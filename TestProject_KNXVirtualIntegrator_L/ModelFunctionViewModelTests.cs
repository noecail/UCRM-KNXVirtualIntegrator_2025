using System.Collections.ObjectModel;

namespace TestProject_KNXVirtualIntegrator_L;

public class StructureSelectionViewModel
{
    public ObservableCollection<StructureModel> Structures { get; set; } = new();
    public StructureModel? SelectedStructure { get; set; }
}

public class StructureModel
{
    public string? StructureName { get; set; }
    public ObservableCollection<ModelFunction>? AssociatedModels { get; set; }
}

public class IndividualElement
{
    public string Name { get; set; }
    public string Address { get; set; }
    public string Dpt { get; set; }
    public string Value { get; set; }

    public IndividualElement(string name, string address, string dpt, string value)
    {
        Name = name;
        Address = address;
        Dpt = dpt;
        Value = value;
    }
}

public class ModelFunction
{
    public ObservableCollection<IndividualElement> Elements { get; set; }

    public ModelFunction(string name, ObservableCollection<IndividualElement> elements)
    {
        Elements = elements;
    }
}
public class ModelFunctionViewModel
{
    public ObservableCollection<ModelFunction> ModelFunctions { get; set; } = new();
    public ModelFunction? SelectedModelFunction { get; set; }
}

public class ModelFunctionViewModelTests
{
    public class FunctionalModelTests
    {
        [Fact]
        public void Test_AddFunctionalModel_AddsToCollection()
        {
            var modelCollection = new ObservableCollection<object>();

            var newModel = new
            {
                Name = "Lumière On/Off",
                Inputs = 1,
                Outputs = 1,
                DptIn = "1.001",
                DptOut = "1.001"
            };

            modelCollection.Add(newModel);

            Assert.Single(modelCollection);
            Assert.Equal("Lumière On/Off", ((dynamic)modelCollection[0]).Name);
        }

        [Fact]
        public void SelectingStructure_ShouldUpdate_ModelList()
        {
            var model1 = new ModelFunction("MF1", new ObservableCollection<IndividualElement>());
            var model2 = new ModelFunction("MF2", new ObservableCollection<IndividualElement>());
            var structure = new StructureModel
            {
                StructureName = "Lumière ON/OFF",
                AssociatedModels = new ObservableCollection<ModelFunction> { model1, model2 }
            };

            var vm = new StructureSelectionViewModel();
            vm.Structures.Add(structure);

            vm.SelectedStructure = structure;

            Assert.Equal(2, vm.SelectedStructure.AssociatedModels.Count);
        }
        
        [Fact]
        public void Test_SelectModelFunction_DisplaysAssociatedIEs()
        {
            var ie1 = new IndividualElement("NomIE1", "0/1/1", "1.001", "true");
            var ie2 = new IndividualElement("NomIE2", "0/1/2", "1.001", "false");

            var elements = new ObservableCollection<IndividualElement> { ie1, ie2 };
            var modelFunction = new ModelFunction("MF_Test", elements);

            var selectedMfViewModel = new ModelFunctionViewModel();
            selectedMfViewModel.ModelFunctions.Add(modelFunction);

            selectedMfViewModel.SelectedModelFunction = modelFunction;

            Assert.Equal(modelFunction, selectedMfViewModel.SelectedModelFunction);
            Assert.Equal(2, selectedMfViewModel.SelectedModelFunction.Elements.Count);

            var firstElement = selectedMfViewModel.SelectedModelFunction.Elements[0];
            Assert.Equal("0/1/1", firstElement.Address);
            Assert.Equal("1.001", firstElement.Dpt);
            Assert.Equal("true", firstElement.Value);

            var secondElement = selectedMfViewModel.SelectedModelFunction.Elements[1];
            Assert.Equal("0/1/2", secondElement.Address);
            Assert.Equal("1.001", secondElement.Dpt);
            Assert.Equal("false", secondElement.Value);
        }
    }
    
    [Fact]
    public void Test_Modify_IE_Address_UpdatesCorrectly()
    {
        var ie = new IndividualElement("NomIE", "0/1/1", "1.001", "true");

        var elements = new ObservableCollection<IndividualElement> { ie };
        var modelFunction = new ModelFunction("MF_Lumière", elements);

        var newAddress = "1/2/3";
        modelFunction.Elements[0].Address = newAddress;

        Assert.Equal(newAddress, modelFunction.Elements[0].Address);
    }
    
    [Fact]
    public void RemoveModelFunction_ShouldRemoveFromAssociatedStructure()
    {
        var model1 = new ModelFunction("MF1", new ObservableCollection<IndividualElement>());
        var model2 = new ModelFunction("MF2", new ObservableCollection<IndividualElement>());
        var structure = new StructureModel
        {
            StructureName = "Stores",
            AssociatedModels = new ObservableCollection<ModelFunction> { model1, model2 }
        };

        var initialCount = structure.AssociatedModels.Count;

        structure.AssociatedModels.Remove(model1);

        Assert.Equal(initialCount - 1, structure.AssociatedModels.Count);
        Assert.DoesNotContain(model1, structure.AssociatedModels);
    }
    
    [Fact]
    public void AddModelCommand_ShouldAdd_NewModelFunction()
    {
        var structure = new StructureModel
        {
            StructureName = "Lumière ON/OFF",
            AssociatedModels = new ObservableCollection<ModelFunction>()
        };

        var initialCount = structure.AssociatedModels.Count;

        var newModel = new ModelFunction("MF_Nouveau", new ObservableCollection<IndividualElement>());
        structure.AssociatedModels.Add(newModel);

        Assert.Equal(initialCount + 1, structure.AssociatedModels.Count);
        Assert.Contains(newModel, structure.AssociatedModels);
    }

    [Fact]
    public void CreateStructureCommand_ShouldAdd_NewStructure()
    {
        var vm = new StructureSelectionViewModel();
        var initialCount = vm.Structures.Count;

        var newStructure = new StructureModel
        {
            StructureName = "Nouvelle Structure",
            AssociatedModels = new ObservableCollection<ModelFunction>()
        };
        vm.Structures.Add(newStructure);

        Assert.Equal(initialCount + 1, vm.Structures.Count);
        Assert.Contains(newStructure, vm.Structures);
    }
    
    [Fact]
    public void InteractionSequence_ShouldRemainConsistent()
    {
        // Arrange
        var ie = new IndividualElement("IE1", "0/0/1", "1.001", "false");
        var mf = new ModelFunction("MF_Auto", new ObservableCollection<IndividualElement> { ie });
        var structure = new StructureModel
        {
            StructureName = "Test Structure",
            AssociatedModels = new ObservableCollection<ModelFunction>()
        };

        var vm = new StructureSelectionViewModel();
        vm.Structures.Add(structure);

        // Act
        vm.SelectedStructure = structure;
        structure.AssociatedModels.Add(mf);
        var selectedMF = structure.AssociatedModels[0];

        // Assert
        // On vérifie qu'on manipule bien le même modèle
        Assert.Same(mf, selectedMF);

        // Puis on vérifie les propriétés du premier IE
        Assert.Equal("IE1", selectedMF.Elements[0].Name);
        Assert.Equal("0/0/1", selectedMF.Elements[0].Address);
        Assert.Equal("1.001", selectedMF.Elements[0].Dpt);
        Assert.Equal("false", selectedMF.Elements[0].Value);
    }

    [Fact]
    public void Test_Modify_IE_WithInvalidAddress_ShouldFail()
    {
        var ie = new IndividualElement("IE1", "INVALID", "1.001", "true");
        var model = new ModelFunction("MF_InvalidAddr", new ObservableCollection<IndividualElement> { ie });

        var gaOk = System.Text.RegularExpressions.Regex.IsMatch(model.Elements[0].Address ?? "", 
            @"^(?:\d|[12]\d|3[01])\/(?:\d|[1-7])\/(?:\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])$");

        Assert.False(gaOk, $"Address '{model.Elements[0].Address}' should be invalid");
    }

    [Fact]
    public void Test_Modify_IE_WithInvalidDpt_ShouldFail()
    {
        var ie = new IndividualElement("IE1", "1/2/3", "BAD_DPT", "false");
        var model = new ModelFunction("MF_InvalidDpt", new ObservableCollection<IndividualElement> { ie });

        var dptOk = System.Text.RegularExpressions.Regex.IsMatch(model.Elements[0].Dpt ?? "", @"^\d{1,2}\.\d{3}$");

        Assert.False(dptOk, $"DPT '{model.Elements[0].Dpt}' should be invalid");
    }


    [Fact]
    public void Test_Duplicate_IE_Address_ShouldBeDetected()
    {
        var ie1 = new IndividualElement("IE1", "1/2/3", "1.001", "true");
        var ie2 = new IndividualElement("IE2", "1/2/3", "1.001", "false");
        var model = new ModelFunction("MF_Duplicate", new ObservableCollection<IndividualElement> { ie1, ie2 });

        var dupes = model.Elements
            .GroupBy(e => e.Address)
            .Where(g => !string.IsNullOrWhiteSpace(g.Key) && g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        Assert.Single(dupes);
        Assert.Equal("1/2/3", dupes[0]);
    }

    [Fact]
    public void Test_Remove_IE_FromModelFunction()
    {
        var ie = new IndividualElement("IE1", "0/0/1", "1.001", "true");
        var model = new ModelFunction("MF_RemoveIE", new ObservableCollection<IndividualElement> { ie });

        model.Elements.Remove(ie);

        Assert.Empty(model.Elements);
    }

    [Fact]
    public void Test_SelectEmptyModelFunction_ShouldHandleGracefully()
    {
        var model = new ModelFunction("MF_Empty", new ObservableCollection<IndividualElement>());
        var vm = new ModelFunctionViewModel();
        vm.ModelFunctions.Add(model);

        vm.SelectedModelFunction = model;

        Assert.NotNull(vm.SelectedModelFunction);
        Assert.Empty(vm.SelectedModelFunction.Elements);
    }

    [Fact]
    public void Test_Filter_IEs_ByAddress()
    {
        var ie1 = new IndividualElement("IE1", "1/1/1", "1.001", "true");
        var ie2 = new IndividualElement("IE2", "1/1/2", "1.001", "false");
        var model = new ModelFunction("MF_Filter", new ObservableCollection<IndividualElement> { ie1, ie2 });

        var filtered = model.Elements.Where(e => e.Address == "1/1/1").ToList();

        Assert.Single(filtered);
        Assert.Equal("IE1", filtered[0].Name);
    }

    [Fact]
    public void Test_Add_IE_WithEmptyFields_ShouldFail()
    {
        var ie = new IndividualElement("", "", "", "");
        var model = new ModelFunction("MF_Invalid", new ObservableCollection<IndividualElement> { ie });

        // DPT attendu: "x.xxx" ; Adresse: "main/middle/sub"
        var dptOk = System.Text.RegularExpressions.Regex.IsMatch(model.Elements[0].Dpt ?? "", @"^\d{1,2}\.\d{3}$");
        var gaOk  = System.Text.RegularExpressions.Regex.IsMatch(model.Elements[0].Address ?? "", 
            @"^(?:\d|[12]\d|3[01])\/(?:\d|[1-7])\/(?:\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])$");

        Assert.True(string.IsNullOrWhiteSpace(model.Elements[0].Name), "Name should be empty");
        Assert.False(gaOk,  $"Address '{model.Elements[0].Address}' should be invalid");
        Assert.False(dptOk, $"DPT '{model.Elements[0].Dpt}' should be invalid");
    }

}
