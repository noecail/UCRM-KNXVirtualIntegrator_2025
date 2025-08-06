using System.Collections.ObjectModel;
using KNX_Virtual_Integrator.Model.Entities;
using KNX_Virtual_Integrator.Model.Interfaces;
using Knx.Falcon;


namespace KNX_Virtual_Integrator.Model.Implementations;

public class Analyze(ObservableCollection<FunctionalModel>  liste, IGroupCommunication communication) : IAnalyze
{
    public readonly ObservableCollection<FunctionalModel> FunctionalModels = liste;
    public List<List<List<List<ResultType>>>> Results { get; set; } = []; //Table of results sorted by Tests, in TestedElements, in functionalModels
    public IGroupCommunication Communication = communication;

    
    /// <summary>
    /// Tests all the functional models of a list and updates the table of results
    /// </summary>
    public async Task TestAll()
    {
        Results = [];
        List<List<List<List<ResultType>>>> resList = [];
        foreach (var functionalModelToTest in FunctionalModels) //
        {
                var res = await TestModel(functionalModelToTest);
                resList.Add(res);
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
        var result = new List<List<List<ResultType>>>();
        foreach (var element in functionalModel.ElementList)
        {
            var res = await TestElement(element);
            result.Add(res);
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
        
        var time = 2000;
        foreach (var t in testsCmd) // parcourt toutes les colonnes de Cmd et s'il y a un dpt 5, mettre à 5000 ms d'attente
            if (t.Type == 5) 
                time = 5000;

        for (var i = 0; i < testsCmd[0].Value.Count; i++) //For each of the tests (1 test per value in the Cmd part) (parcourt les lignes)
        {
            var testResult = ResultType.Failure; //Result of the test
            List<ResultType> resList=[]; //List to check each expected dpt initialized at 0
            for (var j = 0; j < testsIe?.Count; j++) //Initialization of the list to false for each value
            {
                resList.Add(ResultType.Failure);
            }
            
            //var cts = new CancellationTokenSource();
            List<Task<List<GroupCommunication.GroupMessage>>> readTaskList = [];
            if (testsIe != null)
            {
                foreach (var ie in testsIe) //Start all the tasks to read 
                    readTaskList.Add(Communication.GroupValuesTimerOrRecievedAWriteAsync(ie.Address, time));
            }

            foreach(var test in testsCmd) //Send all the commands 
            {
                await Communication.GroupValueWriteAsync(test.Address, test.Value[i]!);
            }
            while (readTaskList.TrueForAll(task => task.Result.Count != 0) && testResult == ResultType.Failure) //While the timer hasn't expired and the test hasn't succeeded
            {
                for (var j = 0; j < testsIe?.Count; j++) //Goes through all the expected returns (columns)
                {
                    if (testsIe[j].Value[i] != null && resList[j] == ResultType.Failure)
                    {
                        var readValues = readTaskList[j].Result; //Updates readValues, the list of messages read on the bus
                        resList[j] = CheckResult(ref readValues, testsIe[j], i); //Check if the message has arrived
                    }
                    testResult = resList[j];
                }
            }

            if (!readTaskList[0].IsCompleted) //If the test succeeded before the end of the timer
            {
                //TO DO :Arrêter les tâches
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
        if (alternateResult.Equals(ResultType.Success))
            result = ResultType.Response;
        return result;
    }
}

public enum ResultType {Success, Response, Failure}