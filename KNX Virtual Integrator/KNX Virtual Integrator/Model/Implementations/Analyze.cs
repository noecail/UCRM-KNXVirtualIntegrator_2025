using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using KNX_Virtual_Integrator.Model.Entities;
using KNX_Virtual_Integrator.Model.Interfaces;
using Knx.Falcon;


namespace KNX_Virtual_Integrator.Model.Implementations;
/// <summary>
/// Class used to analyze the knx installation according to a list of functional models.
/// The models each have a list of Element, which have a list of commands and expected results.
/// The results are thus listed as a list of (models)lists of (elements)lists of (commands)lists of (expected results)<see cref="ResultType"/>.
/// </summary>
/// <param name="communication">The class handling the messages to send and the reception</param>
public class Analyze(IGroupCommunication communication) : IAnalyze
{
    /// <summary>
    /// The list of models to test
    /// </summary>
    public ObservableCollection<FunctionalModel> FunctionalModels = [];
    /// <summary>
    /// Table of results sorted by Tests, in TestedElements, in functionalModels
    /// </summary>
    public List<List<List<List<ResultType>>>> Results { get; set; } = [];
    /// <summary>
    /// The class handling the messages to send and the reception
    /// </summary>
    public IGroupCommunication Communication = communication;
    
    /// <summary>
    /// Default at 2000 ms and set when calling <see cref="TestAll"/>.
    /// Used to set an all around timeout for commands.
    /// </summary>
    private int _timeout = 2000;
    
    /// <summary>
    /// Default at 0 ms and set when calling <see cref="TestAll"/>.
    /// Used to space out tests, to not saturate the installation.
    /// </summary>
    private int _latency;
    
    /// <summary>
    /// Event that occurs when the model analysis state changes (Between waiting, running, finished and none).
    /// Not yet implemented.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>
    /// Invokes <see cref="PropertyChanged"/> when called.
    /// </summary>
    /// <param name="propertyName">The name of the property that was changed.</param>
    private void OnPropertyChanged(string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    /// <summary>
    /// Tests all the functional models of a list and updates the table of results
    /// </summary>
    public async Task TestAll(ObservableCollection<FunctionalModel> listModels, int timeout, int latency)
    {
        FunctionalModels = listModels;
        
        _timeout = timeout>=0?timeout:_timeout;
        _latency = latency>0?latency:_latency;
        Results = [];
        List<List<List<List<ResultType>>>> resList = [];
        foreach (var functionalModelToTest in FunctionalModels) //
        {
                OnPropertyChanged("Running");
                var res = await TestModel(functionalModelToTest);
                resList.Add(res);
                OnPropertyChanged("Finished");
        }
        Results = resList;
    }

    /// <summary>
    /// Tests all the TestedElement from a given FunctionalModel
    /// </summary>
    /// <param name="functionalModel">FunctionalModel to test</param>
    /// <returns></returns>
    private async Task<List<List<List<ResultType>>>> TestModel(FunctionalModel functionalModel)
    {
        functionalModel.UpdateValue();
        var result = new List<List<List<ResultType>>>();
        foreach (var element in functionalModel.ElementList)
        {
            if (_latency > 0)
            {
                var res = await TestElement(element);
                result.Add(res);
                Task.Delay(_latency).Wait();
            }
            else
            {
                var res = await TestElement(element);
                result.Add(res);
            }
        }

        return result;
    }
    
    /// <summary>
    /// Realizes all the tests of a TestedElement
    /// </summary>
    /// <param name="element">Element to be tested</param>
    /// <returns></returns>
    private async Task<List<List<ResultType>>> TestElement(TestedElement element)
    {
        var testsCmd = element.TestsCmd;
        var testsIe = element.TestsIe;
        List<List<ResultType>> result = []; //List containing results of each of the tests

        for (var i = 0; i < testsCmd[0].Value.Count; i++) //For each of the tests (1 test per value in the Cmd part) (parcourt les lignes)
        {
            var testResult = ResultType.Failure; //Result of the test
            List<ResultType> resList=[]; //List to check each expected dpt initialized at 0
            for (var j = 0; j < testsIe?.Count; j++) //Initialization of the list to false for each value
            {
                resList.Add(ResultType.Failure);
            }
            
            List<Task<List<GroupCommunication.GroupMessage>>> readTaskList = [];
            if (testsIe != null)
                foreach (var ie in testsIe) //Start all the tasks to read 
                {
                    if (ie.Address == "") continue;
                    readTaskList.Add(Communication.GroupValuesTimerOrRecievedAWriteAsync(ie.Address, _timeout));
                }
            else
                testResult = ResultType.Success;

            foreach(var test in testsCmd) //Send all the commands 
            {
                if (test.Address == "") break;
                await Communication.GroupValueWriteAsync(test.Address, test.Value[i]!);
            }
            while (readTaskList.TrueForAll(task => task.Result.Count != 0) && testResult == ResultType.Failure && readTaskList.Count>0) //While the timer hasn't expired and the test hasn't succeeded
            {
                for (var j = 0; j < testsIe?.Count; j++) //Goes through all the expected returns (columns)
                {
                    if (testsIe[j].Address == "")
                    {
                        resList[j] = ResultType.Failure;
                    } 
                    else if (testsIe[j].Value[i] != null && resList[j] == ResultType.Failure)
                    {
                        var readValues = readTaskList[j].Result; //Updates readValues, the list of messages read on the bus
                        resList[j] = CheckResult(ref readValues, testsIe[j], i); //Check if the message has arrived
                    } 
                    else if (testsIe[j].Value[i] is null)
                    {
                        resList[j] = ResultType.Success;
                    }
                    testResult = resList[j];
                }
            }
            result.Add(resList); //Adds the result of the test to the list of results of tests
        }
        return result;
    }

    /// <summary>
    /// Checks if the value of an index of a DataPointType is in a list of GroupMessage
    /// </summary>
    /// <param name="readValues">List of values read on the bus</param>
    /// <param name="expectedResult">DataPointType where the value has to be found</param>
    /// <param name="index">Index of the test</param>
    /// <returns></returns>
    private static ResultType CheckResult(ref List<GroupCommunication.GroupMessage> readValues, DataPointType expectedResult, int index)
    {
        var result = ResultType.Failure;
        var alternateResult = ResultType.Failure;
        var i=0;
        if (expectedResult.Value[index] == null)
            result = ResultType.Success;
        while (!result.Equals(ResultType.Success) && i < readValues.Count)
        {
            var value =  readValues[i];
            if (value.DestinationAddress == expectedResult.Address
                && value.Value is not null
                && value.EventType is GroupEventType.ValueWrite 
                && value.Value.Equals(expectedResult.Value[index]))
            {
                result = ResultType.Success;
            } 
            else if (value.DestinationAddress == expectedResult.Address
                       && value.Value is not null
                       && value.EventType is GroupEventType.ValueResponse)
            {
                result = ResultType.Response;
            } 
            else if (value.DestinationAddress == expectedResult.Address
                       && value.EventType is GroupEventType.ValueWrite 
                       && (value.Value is null || !value.Value.Equals(expectedResult.Value[index])))
            {
                alternateResult = ResultType.Success;
            }
            i++;
        }
        if (alternateResult.Equals(ResultType.Success) && !result.Equals(ResultType.Success))
            result = ResultType.Response;
        return result;
    }
}

/// <summary>
/// The 3 possible types of results : <see cref="Success"/>, <see cref="Response"/> and <see cref="Failure"/>.
/// </summary>
public enum ResultType
{
    /// <summary> The result when the expected value is received (or if no value is expected).</summary>
    Success, 
    /// <summary> The result when the expected value is not received but there is an answer.</summary>
    Response, 
    /// <summary> The result when the expected value is not received from the correct address
    /// until the time runs out. Or if there is an error.</summary>
    Failure
}