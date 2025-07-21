using KNX_Virtual_Integrator.Model.Entities;
using KNX_Virtual_Integrator.Model.Interfaces;
using Knx.Falcon;


namespace KNX_Virtual_Integrator.Model.Implementations;

public class Analyze(FunctionalModelList liste, GroupCommunication communication) : IAnalyze
{
    public readonly List<List<FunctionalModel>> FunctionalModels = liste.FunctionalModels;
    public List<List<List<List<List<bool>>>>> Results { get; set; } = []; //Table of results sorted by Tests, in TestedElements, in functionalModels
    public GroupCommunication Communication = communication;

    
    /// <summary>
    /// Tests all the functional models of a list and updates the table of results
    /// </summary>
    public async Task TestAll()
    {
        Results = [];
        foreach (var functionalModelStructure in FunctionalModels) //
        {
            List<List<List<List<bool>>>> resList = [];
            foreach (var model in functionalModelStructure)
            {
                var res = await TestModel(model);
                resList.Add(res);
            }
            Results.Add(resList);
        }
    }

    /// <summary>
    /// Tests all the TestedElement from a given FunctionalModel
    /// </summary>
    /// <param name="functionalModel">FunctionalModel to test</param>
    /// <returns></returns>
    private async Task<List<List<List<bool>>>> TestModel(FunctionalModel functionalModel)
    {
        var result = new List<List<List<bool>>>();
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
    private async Task<List<List<bool>>> TestElement(TestedElement element)
    {
        var testsCmd = element.TestsCmd;
        var testsIe = element.TestsIe;
        List<List<bool>> result = []; //List containing results of each of the tests
        for (var i = 0; i < testsCmd[0].Value.Count; i++) //For each of the tests
        {
            var time = 1000;
            if (testsCmd[i].Type == 5) 
                time = 5000;
            var testResult = false; //Result of the test
            List<bool> resList=[]; //List to check each expected dpt initialized at 0
            for (var j = 0; j < (testsIe?[0].Value.Count); j++)
            {
                resList.Add(false);
            }
            //var cts = new CancellationTokenSource();
            List<Task<List<GroupCommunication.GroupMessage>>> readTaskList = [];
            if (testsIe != null)
            {
                foreach (var ie in testsIe) //Start all the tasks to read 
                    readTaskList.Add(Communication.GroupValuesWithinTimerAsync(ie.Address, time));
            }

            foreach(var test in testsCmd) //Send all the commands
            {
                await Communication.GroupValueWriteAsync(test.Address, test.Value[i]!);
            }
            while (!readTaskList[0].IsCompleted || testResult) //While the timer hasn't expired and the test hasn't succeeded
            {
                for (var j = 0; j < testsIe?[0].Value.Count; j++) //Goes through all the expected returns
                {
                    if (testsIe[j].Value[i] != null && !resList[j])
                    {
                        //If the message hasn't been found yet.
                        var readValues = readTaskList[j].Result; //Updates readValues, the list of messages read on the bus
                        resList[j] = CheckResult(ref readValues, testsIe[j], i); //Check if the message has arrived
                    }
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