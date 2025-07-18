using KNX_Virtual_Integrator.Model.Entities;
using KNX_Virtual_Integrator.Model.Interfaces;
using Knx.Falcon;


namespace KNX_Virtual_Integrator.Model.Implementations;

public class Analyze(FunctionalModelList liste, GroupCommunication communication) : IAnalyze
{
    public readonly List<FunctionalModel> FunctionalModels = liste.FunctionalModels;
    public List<List<List<bool>>> Results { get; set; } = []; //Table of results sorted by Tests, in TestedElements, in functionalModels
    public GroupCommunication Communication = communication;

    
    /// <summary>
    /// Tests all the functional models of a list and updates the table of results
    /// </summary>
    public async Task TestAll()
    {
        Results = [];
        foreach (var model in FunctionalModels)
        {
            var res = await TestModel(model);
            Results.Add(res);
        }
    }

    /// <summary>
    /// Tests all the TestedElement from a given FunctionalModel
    /// </summary>
    /// <param name="functionalModel">FunctionalModel to test</param>
    /// <returns></returns>
    private async Task<List<List<bool>>> TestModel(FunctionalModel functionalModel)
    {
        var result = new List<List<bool>>();
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
    private async Task<List<bool>> TestElement(TestedElement element)
    {
        var tests = element.Tests;
        var nbCmd = element.NbCmd;
        List<bool> result = []; //List containing results of each of the tests
        var time = 1000;
        if (tests[0].Type == 5) 
            time = 5000;
        for (var i = 0; i < tests?[0].Value.Count; i++) //For each of the tests
        {
            var testResult = false; //Result of the test
            List<bool> resList=[]; //List to check each expected dpt initialized at 0
            for (var j = nbCmd; j < (tests[0].Value.Count); j++)
            {
                resList.Add(false);
            }
            //var cts = new CancellationTokenSource();
            var readTask = Communication.GroupValuesWithinTimerAsync(tests[nbCmd].Address,time);
            for (var j = 0; j < nbCmd; j++) //Send all the commands
            {
                if (tests != null && tests[j].Value.Count > i && tests[j].Value[i] != null)
                    await Communication.GroupValueWriteAsync(tests[j].Address, tests[j].Value[i]!);
            }
            while (!readTask.IsCompleted || testResult) //While the timer hasn't expired and the test hasn't succeeded
            {
                var readValues = readTask.Result; //Updates readValues, the list of messages read on the bus
                for (var j = nbCmd; j < (tests?[0].Value.Count); j++) //Goes through all the expected returns
                {
                    if (tests[i].Value[j] != null && !resList[j])  //If the message hasn't been found yet.
                        resList[j - nbCmd] = CheckResult(ref readValues, tests[i], j); //Check if the message has arrived
                    testResult = true;
                    foreach (var res in resList) //Check if all the messages of the test expected to be read have been read
                    {
                        testResult = testResult && res;
                    }
                }
            }

            if (!readTask.IsCompleted) //If the test succeeded before the end of the timer
            {
                //TO DO :Arrêter la tâche
            }
            result.Add(testResult); //Adds the result of the test to the list of results of tests
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
    private static bool CheckResult(ref List<GroupCommunication.GroupMessage> readValues, DataPointType expectedResult, int index)
    {
        var result = false;
        var i=0;
        if (expectedResult.Value[index] == null)
            result = true;
        while (result == false && i < readValues.Count)
        {
            var value =  readValues[i];
            if ((value.DestinationAddress == expectedResult.Address) &&
                (value.EventType == GroupEventType.ValueResponse || value.EventType == GroupEventType.ValueWrite) &&
                (value.Value != null)&&(value.Value.Equals(expectedResult.Value[index])))
            {
                result = true;
                readValues.RemoveAt(i);
            }
            i++;
        }
        return result;
    }
}