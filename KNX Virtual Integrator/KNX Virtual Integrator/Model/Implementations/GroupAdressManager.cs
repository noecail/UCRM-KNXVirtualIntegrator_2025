using System.Xml.Linq;
using iText.StyledXmlParser.Jsoup.Select;
using KNX_Virtual_Integrator.Model.Entities;
using KNX_Virtual_Integrator.Model.Interfaces;
using Knx.Falcon;

namespace KNX_Virtual_Integrator.Model.Implementations;
/// <summary>
/// Defines methods for managing group addresses extracted from XML files.
/// </summary>
public class GroupAddressManager(Logger logger, ProjectFileManager projectFileManager, FileLoader loader, NamespaceResolver namespaceResolver, GroupAddressProcessor groupAddressProcessor, GroupAddressMerger groupAddressMerger) : IGroupAddressManager
{
    /// <summary>
    /// Dictionary where each Cmd addresses is grouped with the corresponding Ie adresses
    /// </summary>
    public Dictionary<string, List<XElement>> GroupedAddresses { get; } = new ();
    
    /// <summary>
    /// List of all the Ie addresses
    /// </summary>
    public readonly List<XElement> IeAddressesSet = new();

    private int _groupAddressStructure;
    
    /// <summary>
    /// Group address names prefixes. It is initialized for command
    /// </summary>
    public string[] Prefixes { get; set; } = { "Cmd", "Command", "Control", "Do","on/off", "Variations", "Montee/Descente", "Position", "Valeurs" }; //Initialize the keywords for command

    /// <summary>
    /// Extracts group address information from a specified XML file.
    ///
    /// Determines the file path to use based on user input and whether a specific group address
    /// file is chosen or a default file is used. Depending on the file path, it processes the XML
    /// file to extract and group addresses either from a specific format or a standard format.
    /// </summary>
    public XDocument? ExtractGroupAddress(IFunctionalModelList functionalModelList)
    {
        if (projectFileManager is not { } manager) return null;
        
        // Prend le bon fichier xml en fonction de si l'installateur a importé le projet ou un fichier d'adresse de groupe
        var filePath = App.WindowManager != null && App.WindowManager.MainWindow.UserChooseToImportGroupAddressFile
            ? manager.GroupAddressFilePath
            : manager.ZeroXmlPath;

        var groupAddressFile = loader.LoadXmlDocument(filePath);
        if (groupAddressFile == null) return null;
        
        namespaceResolver.SetNamespaceFromXml(filePath);

        if (namespaceResolver.GlobalKnxNamespace == null) return null;

        if (filePath == manager.ZeroXmlPath) //If the processes difle is a ets project
        {
            groupAddressFile = NewProcessZeroXmlFile(groupAddressFile, functionalModelList);
        }
        else //If it is a group address file
        {
            NewProcessStandardXmlFile(groupAddressFile.Root?.Elements(), functionalModelList);
        }
        return groupAddressFile;
    }


    /// <summary>
    /// Processes an XML file in the Zero format to extract and group addresses.
    ///
    /// This method extracts device references and their links, processes group addresses, and 
    /// groups them based on device links and common names. It handles the creation and updating 
    /// of grouped addresses, avoiding name collisions by appending suffixes if necessary.
    ///
    /// <param name="groupAddressFile">The XML document containing group address data in Zero format.</param>
    /// <param name="functionalModelList">The list of functional models to fill</param>
    /// </summary>
    public XDocument? NewProcessZeroXmlFile(XDocument groupAddressFile, IFunctionalModelList functionalModelList)
    {
        var doc = groupAddressFile.Root?.Elements();
        _groupAddressStructure = DetermineGroupAddressStructure0Xml(groupAddressFile);
        var ns = namespaceResolver.GlobalKnxNamespace ?? null;
        if (null == ns || doc == null)
            return null;
        var groupAddresses = doc.Descendants(ns + "GroupRanges");

        NewProcessStandardXmlFile(groupAddresses.Elements(), functionalModelList);
        XDocument document = new XDocument(new XElement("Root", groupAddresses));
        return document;

    }
    
    
   /// <summary>
    /// Processes an XML file in the Zero format to extract and group addresses.
    ///
    /// This method extracts device references and their links, processes group addresses, and 
    /// groups them based on device links and common names. It handles the creation and updating 
    /// of grouped addresses, avoiding name collisions by appending suffixes if necessary.
    ///
    /// <param name="groupAddressFile">The XML document containing group address data in Zero format.</param>
    /// <param name="functionalModelList">The list of functional models to fill</param>
    /// </summary>
    public void ProcessZeroXmlFile(XDocument groupAddressFile, IFunctionalModelList functionalModelList)
    {
        // Initialiser un HashSet pour stocker les adresses "Ie" et éviter les doublons
        var ieAddresses = new HashSet<string>();
    
        // Vider les collections avant de commencer le traitement du fichier
        IeAddressesSet.Clear();
        GroupedAddresses.Clear();

        // Détermine la structure des adresses de groupe (2 ou 3 niveaux) pour la conversion
        
        // Étape 1 : Extraire les références des appareils
        var deviceRefs = groupAddressFile.Descendants(namespaceResolver.GlobalKnxNamespace! + "DeviceInstance")
            .Select(di => (
                Id: di.Attribute("Id")?.Value,
                Links: di.Descendants(namespaceResolver.GlobalKnxNamespace! + "ComObjectInstanceRef")
                    .Where(cir => cir.Attribute("Links") != null)
                    .SelectMany(cir => cir.Attribute("Links")?.Value.Split(' ') ?? Array.Empty<string>())
                    .ToHashSet()
            ))
            .ToList();

        // Extrait toutes les adresses de groupes du fichier
        var groupAddresses = groupAddressFile.Descendants(namespaceResolver.GlobalKnxNamespace! + "GroupAddress").ToList();
        // Dictionnaire temporaire pour regrouper les adresses par nom commun, ID de l'appareil et adresse de commande
        var tempGroupedAddresses = new Dictionary<(string CommonName, string DeviceId, string CmdAddress), HashSet<string>>();

        // Étape 2 : Regrouper les adresses par nom commun et ID de l'appareil
        foreach (var ga in groupAddresses)
        {
            var id = ga.Attribute("Id")?.Value;
            var name = ga.Attribute("Name")?.Value;
            
            // Convertir l'adresse au format x/x/x en fonction de la structure d'adresses déterminée
            var address = groupAddressProcessor.DecodeAddress(ga.Attribute("Address")?.Value ?? string.Empty, _groupAddressStructure); 

            // Si l'adresse n'est pas vide après la conversion, on la met à jour dans l'élément XML
            if (address != String.Empty)
            {
                ga.Attribute("Address")!.Value = address;
            }

            // Si l'identifiant ou le nom est nul, passer à l'itération suivante
            if (id == null || name == null) continue;

            // Récupère l'identifiant GA (Group Address) en supprimant le préfixe s'il existe
            var gaId = id.Contains("GA-") ? id.Substring(id.IndexOf("GA-", StringComparison.Ordinal)) : id;
            // Trouver les appareils liés à cette adresse de groupe
            var linkedDevices = deviceRefs.Where(dr => dr.Links.Contains(gaId));

            foreach (var device in linkedDevices)
            {
                // Sépare les adresses qui commencent par Cmd et celles par Ie, pour plus de personnalisation, on pourrait mettre en option les prefixes Cmd et Ie
                var isCmd = name.StartsWith("Cmd", StringComparison.OrdinalIgnoreCase);
                var isIe = name.StartsWith("Ie", StringComparison.OrdinalIgnoreCase);
                // Extrait le nom commun en enlevant les préfixes "Cmd" ou "Ie"
                var commonName = isCmd
                    ? name.Substring(3)
                    : isIe
                        ? name.Substring(2)
                        : name;

                // Seuls les "Cmd" sont pris en compte pour créer des groupes
                if (isCmd)
                {
                    var key = (CommonName: commonName, DeviceId: device.Id, CmdAddress: address);

                    // Si la clé n'existe pas encore dans le dictionnaire temporaire, l'ajouter
                    if (!tempGroupedAddresses.ContainsKey(key!))
                    {
                        tempGroupedAddresses[key!] = new HashSet<string>();
                    }

                    // Ajouter l'identifiant de l'adresse de groupe à l'ensemble sous la clé appropriée
                    tempGroupedAddresses[key!].Add(id);
                }
                else if (name.StartsWith("Ie", StringComparison.OrdinalIgnoreCase))
                {
                    // Ajouter les "Ie" dans les groupes existants correspondants aux "Cmd"
                    var cmdKey = tempGroupedAddresses.Keys.FirstOrDefault(k => k.CommonName == commonName && k.DeviceId == device.Id);
                    if (cmdKey != default)
                    {
                        tempGroupedAddresses[cmdKey].Add(id);
                    }
                }
                
                // Ajouter les adresses "Ie" à la liste en vérifiant les doublons
                if (isIe)
                {
                    if (ieAddresses.Add(address)) // Essaie d'ajouter l'adresse, retourne vrai si l'adresse n'était pas déjà dans l'ensemble
                    {
                        IeAddressesSet.Add(ga);
                    }
                }
            }
        }

        // Étape 3 : Regrouper les adresses "Cmd" et "Ie" sous la même clé, en tenant compte du DeviceId
        foreach (var entry in tempGroupedAddresses)
        {
            var commonName = $"{entry.Key.CommonName}_{entry.Key.CmdAddress}";
            var gaIds = entry.Value;

            // Chercher l'entrée existante basée sur le commonName et le DeviceId
            var existingEntry = GroupedAddresses.FirstOrDefault(g =>
                gaIds.All(id => g.Value.Any(x => x.Attribute("Id")?.Value == id)) ||
                g.Value.Select(x => x.Attribute("Id")?.Value).All(id => gaIds.Contains(id ?? string.Empty)));

            if (existingEntry.Value != null)
            {
                logger.ConsoleAndLogWriteLine($"Matching or subset found for: {existingEntry.Key}. Adding missing IDs.");

                // Ajouter les IDs manquants à l'entrée existante
                foreach (var gaId in gaIds)
                {
                    if (existingEntry.Value.All(x => x.Attribute("Id")?.Value != gaId))
                    {
                        var ga = groupAddresses.FirstOrDefault(x => x.Attribute("Id")?.Value == gaId);
                        if (ga != null) existingEntry.Value.Add(ga);
                    }
                }
            }
            else
            {
                logger.ConsoleAndLogWriteLine($"Creating a new entry for: {commonName}");
                // Créer une nouvelle entrée pour les adresses "Cmd" et "Ie"
                GroupedAddresses[commonName] = gaIds.Select(id => groupAddresses.First(x => x.Attribute("Id")?.Value == id)).ToList();
            }
        }

        // Étape 4 : Finaliser les regroupements sous les adresses "Cmd"
        foreach (var entry in GroupedAddresses)
        {
            // Trouver la première adresse "Cmd" dans le groupe
            var cmdAddress = entry.Value.FirstOrDefault(x => x.Attribute("Name")?.Value.StartsWith("Cmd", StringComparison.OrdinalIgnoreCase) ?? false );

            if (cmdAddress != null)
            {
                var commonName = entry.Key;
                // Initialise une nouvelle liste avec l'adresse "Cmd"
                GroupedAddresses[commonName] = new List<XElement> { cmdAddress } ;

                // Ajouter toutes les adresses "Ie" correspondantes sous le même nom commun
                GroupedAddresses[commonName].AddRange(entry.Value.Where(x => x.Attribute("Name")?.Value.StartsWith("Ie", StringComparison.OrdinalIgnoreCase) ?? false));
            }
            else
            {
                // Si aucune adresse "Cmd" n'est trouvée, ajouter le groupe tel quel
                GroupedAddresses[entry.Key] = entry.Value;
            }
        }
       
        // Appel optionnel, cela ajoute des Ie qui ressemble à 80% aux groupes de Cmd seules
        // À voir l'appel de la fonction est pertinent
        groupAddressMerger.MergeSingleElementGroups(GroupedAddresses, IeAddressesSet);
        
    }
    
    /// <summary>
    /// Processes an XML file in the standard format to extract and group addresses.
    ///
    /// This method processes group addresses from the XML file, normalizing the names by removing
    /// specific prefixes ("Ie" or "Cmd") and grouping addresses based on the remaining common names.
    ///
    /// <param name="groupAddressFile">The XML document containing group address data in standard format.</param>
    /// </summary>
    public void ProcessStandardXmlFile(XDocument groupAddressFile)
    {
        // Vider les collections avant de commencer le traitement du fichier
        IeAddressesSet.Clear();
        GroupedAddresses.Clear();
        
        // Extrait toutes les adresses de groupes du fichier
        var groupAddresses = groupAddressFile.Descendants(namespaceResolver.GlobalKnxNamespace! + "GroupAddress").ToList();
    
        // Dictionnaire pour stocker les adresses "Ie" par suffixe
        var ieAddresses = new Dictionary<string, List<XElement>>(StringComparer.OrdinalIgnoreCase);
        // Dictionnaire pour stocker les adresses "Cmd" par suffixe
        var cmdAddresses = new Dictionary<string, List<XElement>>(StringComparer.OrdinalIgnoreCase);
        // Dictionnaire pour éviter d'ajouter plusieurs fois la même combinaison suffixe/adresse "Cmd"
        var addedCmdAddresses = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        
        // Parcourt chaque adresse de groupe extraite
        foreach (var ga in groupAddresses)
        {
            var name = ga.Attribute("Name")?.Value;
            var address = ga.Attribute("Address")?.Value;
            
            // Vérifie que les attributs "Name" et "Address" ne sont pas nuls
            if (name != null && address != null)
            {
                if (name.StartsWith("Ie", StringComparison.OrdinalIgnoreCase))
                {
                    var suffix = name.Substring(2); // Extraire le suffixe du nom (après "Ie")
                    
                    // Vérifier si l'adresse est déjà présente dans la liste ieAddressesSet
                    if (IeAddressesSet.All(x => x.Attribute("Address")?.Value != address))
                    {
                        IeAddressesSet.Add(ga); // Ajoute l'adresse à IeAddressesSet

                        // Ajoute l'adresse à ieAddresses sous le suffixe correspondant
                        if (!ieAddresses.ContainsKey(suffix))
                        {
                            ieAddresses[suffix] = new List<XElement>();
                        }
                        ieAddresses[suffix].Add(ga);
                    }
                }
                else if (name.StartsWith("Cmd", StringComparison.OrdinalIgnoreCase))
                {
                    var suffix = name.Substring(3); // Extraire le suffixe du nom (après "Cmd")
                    // Ajoute l'adresse à cmdAddresses sous le suffixe correspondant
                    if (!cmdAddresses.ContainsKey(suffix))
                    {
                        cmdAddresses[suffix] = new List<XElement>();
                    }
                    cmdAddresses[suffix].Add(ga);
                }
            }
        }

        // Parcourt chaque entrée dans cmdAddresses pour les associer aux adresses "Ie" correspondantes
        foreach (var cmdEntry in cmdAddresses)
        {
            var suffix = cmdEntry.Key; // Récupère le suffixe
            var cmds = cmdEntry.Value; // Récupère la liste des adresses "Cmd" associées au suffixe

            // Parcourt chaque adresse "Cmd"
            foreach (var cmd in cmds)
            {
                var address = cmd.Attribute("Address")?.Value;
                
                // Vérifie que l'adresse n'est pas null
                if (address != null)
                {
                    // Initialiser le set pour les adresses "Cmd" déjà ajoutées si nécessaire
                    if (!addedCmdAddresses.ContainsKey(suffix))
                    {
                        addedCmdAddresses[suffix] = new HashSet<string>(StringComparer.OrdinalIgnoreCase); 
                    }

                    // Vérifier si la combinaison suffixe/adresse a déjà été ajoutée
                    if (!addedCmdAddresses[suffix].Contains(address))
                    {
                        addedCmdAddresses[suffix].Add(address); // Marquer cette combinaison comme ajoutée

                        // Si des adresses "Ie" avec le même suffixe existent
                        if (ieAddresses.ContainsKey(suffix))
                        {
                            // Ajoute l'adresse "Cmd" à GroupedAddresses
                            groupAddressProcessor.AddToGroupedAddresses(GroupedAddresses, cmd, $"{suffix}_{address}");
                        
                            // Associe toutes les adresses "Ie" correspondantes à cette adresse "Cmd"
                            foreach (var ieGa in ieAddresses[suffix])
                            {
                                groupAddressProcessor.AddToGroupedAddresses(GroupedAddresses, ieGa, $"{suffix}_{address}");
                            }
                        }
                        else
                        {
                            // Si aucune adresse "Ie" ne correspond, on ajoute uniquement l'adresse "Cmd"
                            groupAddressProcessor.AddToGroupedAddresses(GroupedAddresses, cmd, $"{suffix}_{address}");
                        }
                    }
                }
            }
        }

        // Appel optionnel, cela ajoute des Ie qui ressemble à 80% aux groupes de Cmd seules
        // À voir l'appel de la fonction est pertinent
        groupAddressMerger.MergeSingleElementGroups(GroupedAddresses, IeAddressesSet);
    }
    
    
    /// <summary>
    /// Processes an XML file in the standard format to extract and group addresses.
    ///
    /// This method processes group addresses from the XML file, normalizing the names by removing
    /// specific prefixes ("Ie" or "Cmd") and grouping addresses based on the remaining common names.
    ///
    /// <param name="modelStructures">The XML document containing group address data in standard format.</param>
    /// <param name="functionalModelList">The list of lists of functional models.</param>
    /// </summary>
    public void NewProcessStandardXmlFile(IEnumerable<XElement>? modelStructures, IFunctionalModelList functionalModelList)
    {
        List<DataPointType> lostDataPointTypes = [];
        if (modelStructures != null)
        {
            _groupAddressStructure = DetermineGroupAddressStructureGroupAddressFile(modelStructures); //Determines if the address structure is in 2 or 3 levels
            for (var i = 0; i < functionalModelList.FunctionalModels.Count; i++) //Clears the list of list
            {
                functionalModelList.FunctionalModels[i].Clear();
                functionalModelList.ResetCount(i);
            }
            if (functionalModelList.FunctionalModelDictionary.FunctionalModels.Count >0 && functionalModelList.FunctionalModelDictionary.FunctionalModels[^1].Model.Name.Contains("Unrecognized DPTs",StringComparison.OrdinalIgnoreCase))
            {
                functionalModelList.DeleteFromDictionary(functionalModelList.FunctionalModelDictionary.FunctionalModels.Count-1);
            }
            if (_groupAddressStructure == 3)  //If the address structure is 3 levels
            {
                foreach (var modelStructure in modelStructures)
                {
                    var structureList = modelStructure.Elements().ToList();  //Gets all the elements
                    var modelName = modelStructure.Attribute("Name")?.Value ?? ""; //Gets the name of the model
                    modelName = modelName.Replace(" ", "_");
                    var index = functionalModelList.FunctionalModelDictionary.CheckName(modelName); //Checks if the name is related to any model structure in the dictionary
                    List<FunctionalModel>
                        newFunctionalModels =
                            []; // new list to store all the functional model of the next structure
                    List<(DataPointType,int)> unrecognizedDataPoints = [];
                    if (index != -1) //If the name is recognized
                    {
                        var model = functionalModelList.FunctionalModelDictionary.FunctionalModels[index]; //The structure recognized
                        for (var i = 0; i < structureList.Count; i++) //Takes all the commands
                        {
                            var objectType = structureList[i].Elements().ToList();
                            List<String> names = []; //Processes the prefix and name
                            for (var j = 0; j < objectType.Count; j++)
                            {
                                names.Add(objectType[j].Attribute("Name")?.Value ?? "");
                            }
                            
                            var prefix = FindMajorityPrefix(names);
                            if (names.Count == 1)
                            {
                                names = [];
                                for (var j = 0; j < structureList.Count; j++)
                                {
                                    names.Add(
                                        structureList[j].Elements().ToList()[0].Attribute("Name")?.Value ?? "");
                                }

                                var suffix = FindMajoritySuffix(names);
                                prefix = prefix.Replace(suffix, "");
                            }
                            for (var j = 0; j < objectType.Count; j++)
                            {
                                var dptName = objectType[j].Attribute("Name")?.Value ?? "";
                                dptName = dptName.Replace(" ", "_");
                                var circuitName = dptName;
                                if (!string.IsNullOrEmpty(circuitName) &&
                                    !string.IsNullOrEmpty(prefix)) // && circuitName.Contains(prefix))
                                    circuitName = circuitName.Replace(prefix.Replace(' ','_'), "");  //The prefix is the name since cmd is not written so errors
                                else
                                {
                                    circuitName =
                                        string.Join("_",
                                            circuitName.Split('_')[
                                                1..]); //Takes the name of the object, except the first word 
                                }
                                var modelIndex = FindSuffixInModels(circuitName, newFunctionalModels); //Checks if the circuit name corresponds to one of the new functional models
                                if (modelIndex == -1) //If the name is not recognized, creates a new model and adds all the elements
                                {
                                    newFunctionalModels.Add(new FunctionalModel(modelName+"_"+circuitName));
                                    for (var k = 0; k < model.ModelStructure.Count; k++)
                                    {
                                        newFunctionalModels[^1].AddElement();
                                        foreach (var cmd in model.ModelStructure[k].Cmd)
                                        {
                                            newFunctionalModels[^1].ElementList[^1].TestsCmd.Add(new DataPointType());//Adds a command for every command expected
                                        }
                                        foreach (var ie in model.ModelStructure[k].Ie)
                                        {
                                            newFunctionalModels[^1].ElementList[^1].TestsIe.Add(new DataPointType());//Adds an ie for every command expected
                                        }
                                    }
                                    modelIndex = newFunctionalModels.Count - 1;
                                }
                                
                                
                                //Add the dpt wherever needed
                                var newType = int.Parse(objectType[j].Attribute("DPTs")?.Value
                                    .Split('-')[1] ?? "0"); //gets the type between the dashes in the xml
                                var newAddress = objectType[j].Attribute("Address")?.Value ?? "Error";
                                var newDpt = new DataPointType(newType, newAddress, [], dptName); //Builds the dpt
                                var dptKey = model.FindKeyWithKeywords(prefix); //Checks if the dpt name corresponds to a key in the dictionary of the structure

                                if (dptKey == -1) //If the dpt is not recognized, adds it to a list of unrecognzed dpts
                                {
                                    unrecognizedDataPoints.Add((newDpt,modelIndex));
                                    continue;
                                }

                                newFunctionalModels[modelIndex].BuildFromStructure(model, newDpt,dptKey);
                            }
                        }

                        foreach (var dpt in unrecognizedDataPoints)//Checks again if the unrecognized dpt corresponds to missing dpt in the model
                        {
                            var key = newFunctionalModels[dpt.Item2].FindKey(model, dpt.Item1);
                            if (key == -1)
                            {
                                lostDataPointTypes.Add(dpt.Item1);
                                continue;
                            }
                            newFunctionalModels[dpt.Item2].BuildFromStructure(model, dpt.Item1,key);

                        }
                        
                    }
                    else
                    {
                        for (var i = 0; i < structureList.Count; i++) //Takes all the commands
                        {
                            var objectType = structureList[i].Elements().ToList();
                            List<String> names = [];
                            for (var j = 0; j < objectType.Count; j++)
                            {
                                names.Add(objectType[j].Attribute("Name")?.Value ?? "");
                            }

                            var prefix = FindMajorityPrefix(names);
                            if (names.Count == 1)
                            {
                                names = [];
                                for (var j = 0; j < structureList.Count; j++)
                                {
                                    names.Add(
                                        structureList[j].Elements().ToList()[0].Attribute("Name")?.Value ?? "");
                                }

                                var suffix = FindMajoritySuffix(names);
                                if (!string.IsNullOrEmpty(suffix))
                                {
                                    prefix = prefix.Replace(suffix, "");
                                }
                            }                            {
                                for (var j = 0; j < objectType.Count; j++)
                                {
                                    var dptName = objectType[j].Attribute("Name")?.Value ?? "";
                                    dptName = dptName.Replace(" ", "_");
                                    var circuitName = dptName;
                                    if (!string.IsNullOrEmpty(circuitName) &&
                                        !string.IsNullOrEmpty(prefix)) // && circuitName.Contains(prefix))
                                        circuitName = circuitName.Replace(prefix.Replace(' ','_'), "");
                                    else
                                    {
                                        circuitName =
                                            string.Join("_",
                                                circuitName.Split('_')[
                                                    1..]); //Takes the name of the object, except the first word 
                                    }

                                    var modelIndex = FindSuffixInModels(circuitName, newFunctionalModels);
                                    if (modelIndex == -1 && (dptName.Contains("stop",
                                            StringComparison
                                                .OrdinalIgnoreCase) ||Prefixes.Any(p =>
                                            objectType[j].Attribute("Name")?.Value
                                                .StartsWith(p, StringComparison.OrdinalIgnoreCase) == true)))
                                    {
                                        newFunctionalModels.Add(new FunctionalModel(modelName+"_"+circuitName));
                                        modelIndex = newFunctionalModels.Count - 1;
                                    }
                                    
                                    var newType = int.Parse(objectType[j].Attribute("DPTs")?.Value
                                            .Split('-')[1] ?? "0"); //gets the type between the dashes in the xml
                                    var newAddress = objectType[j].Attribute("Address")?.Value ?? "Error";
                                    if (dptName.Contains("stop",
                                            StringComparison
                                                .OrdinalIgnoreCase)) //If it's a stop command, create a new element (which is a copy of the first element)
                                    {
                                        if (newFunctionalModels[j].ElementList.Count >= 1)
                                        {
                                            var baseIndex = 0;
                                            for (var k = 0; k < newFunctionalModels[j].ElementList.Count; k++)
                                            {
                                                if (newFunctionalModels[j].ElementList[k]
                                                    .IsEqual(new TestedElement([1], [""], [[]]))) //Takes the first boolean cmd supposing it is the on/off command
                                                {
                                                    baseIndex = k;
                                                    break;
                                                }
                                            }

                                            newFunctionalModels[j].AddElement(new TestedElement([1],
                                                [newFunctionalModels[j].ElementList[baseIndex].TestsCmd[0].Address], [
                                                    []
                                                ], [newFunctionalModels[j].ElementList[baseIndex].TestsCmd[0].Name],
                                                circuitName));
                                            newFunctionalModels[j].ElementList[^1]
                                                .AddDptToCmd(newType, newAddress, dptName, []);
                                        }
                                    }
                                    else if (Prefixes.Any(p =>
                                                 objectType[j].Attribute("Name")?.Value
                                                     .StartsWith(p, StringComparison.OrdinalIgnoreCase) == true))
                                    {
                                        newFunctionalModels[modelIndex].AddElement(new TestedElement([newType], [newAddress],
                                                [[]], [dptName], circuitName));
                                    }
                                }
                            }
                        }
                        if (newFunctionalModels.Count > 0)
                        {
                            index = functionalModelList.FunctionalModelDictionary.HasSameStructure(
                                newFunctionalModels[0]);
                        }

                        if (index != -1)
                        {
                            var model = functionalModelList.FunctionalModelDictionary.FunctionalModels[index];
                            foreach(var newModel in newFunctionalModels)
                            {
                                newModel.BuildCmdIntValues(model.Model);
                            }
                            for (var i = 0; i < structureList.Count; i++) //Goes through all structures
                            {
                                var objectType = structureList[i].Elements().ToList();
                                List<String> names = [];
                                for (var j = 0; j < objectType.Count; j++)
                                {
                                    names.Add(objectType[j].Attribute("Name")?.Value ?? "");
                                }

                                var prefix = FindMajorityPrefix(names);
                                if (names.Count == 1)
                                {
                                    names = [];
                                    for (var j = 0; j < structureList.Count; j++)
                                    {
                                        names.Add(
                                            structureList[j].Elements().ToList()[0].Attribute("Name")?.Value ?? "");
                                    }

                                    var suffix = FindMajoritySuffix(names);
                                    if (!string.IsNullOrEmpty(suffix))
                                        prefix = prefix.Replace(suffix, "");
                                }

                                for (var j = 0; j < objectType.Count; j++)
                                {
                                    var dptName = objectType[j].Attribute("Name")?.Value ?? "";
                                    dptName = dptName.Replace(" ", "_");
                                    if (Prefixes.All(p =>
                                            !dptName
                                                .StartsWith(p, StringComparison.OrdinalIgnoreCase))
                                        && !dptName
                                            .Contains("stop",
                                                StringComparison
                                                    .OrdinalIgnoreCase)) //If the name doesn't start with anything command related nor contains stop, it's an IE
                                    {
                                        var circuitName = dptName;
                                        if (!string.IsNullOrEmpty(circuitName) &&
                                            !string.IsNullOrEmpty(prefix))
                                            circuitName = circuitName.Replace(prefix.Replace(" ", "_"), "");
                                        else
                                        {
                                            circuitName =
                                                string.Join("_",
                                                    circuitName.Split('_')[
                                                        1..]); //Takes the name of the object, except the first word 
                                        }

                                        var newType = int.Parse(objectType[j].Attribute("DPTs")?.Value
                                            .Split('-')[1] ?? "0"); //gets the type between the dashes in the xml
                                        var newAddress = objectType[j].Attribute("Address")?.Value ?? "Error";
                                        var newDpt = new DataPointType(newType, newAddress, [], dptName);
                                        var modelIndex = FindSuffixInModels(circuitName, newFunctionalModels);
                                        if (modelIndex == -1) //When the circuit name doesn't exist, maybe take j?? dangerous
                                        {
                                            lostDataPointTypes.Add(newDpt);
                                            continue;
                                        }
                                        var dptKey = model.FindKeyWithKeywords(prefix);
                                        if(dptKey == -1) 
                                            dptKey = newFunctionalModels[modelIndex].FindKey(model,newDpt);
                                        if (dptKey == -1)
                                        {
                                            unrecognizedDataPoints.Add((newDpt,modelIndex));
                                            continue;
                                        }
                                        newFunctionalModels[modelIndex].BuildFromStructure(model,newDpt,dptKey);
                                    }
                                }
                            } 
                            
                            foreach (var dpt in unrecognizedDataPoints)//Checks again if the unrecognized dpt corresponds to missing dpt in the model
                            {
                                var key = newFunctionalModels[dpt.Item2].FindKey(model, dpt.Item1);
                                if (key == -1)
                                {

                                    lostDataPointTypes.Add(dpt.Item1);
                                    continue;
                                }
                                newFunctionalModels[dpt.Item2].BuildFromStructure(model, dpt.Item1,key);

                            }

                        }
                        else
                        {
                            for (var i = 0; i < structureList.Count; i++) //Goes through all structures
                            {
                                var objectType = structureList[i].Elements().ToList();
                                List<String> names = [];
                                for (var j = 0; j < objectType.Count; j++)
                                {
                                    names.Add(objectType[j].Attribute("Name")?.Value ?? "");
                                }

                                var prefix = FindMajorityPrefix(names);
                                if (names.Count == 1)
                                {
                                    names = [];
                                    for (var j = 0; j < structureList.Count; j++)
                                    {
                                        names.Add(
                                            structureList[j].Elements().ToList()[0].Attribute("Name")?.Value ?? "");
                                    }

                                    var suffix = FindMajoritySuffix(names);
                                    if (!string.IsNullOrEmpty(suffix))
                                        prefix = prefix.Replace(suffix, "");
                                }

                                for (var j = 0; j < objectType.Count; j++)
                                {
                                    var dptName = objectType[j].Attribute("Name")?.Value ?? "";
                                    dptName = dptName.Replace(" ", "_");
                                    if (Prefixes.All(p =>
                                            !(objectType[j].Attribute("Name")?.Value
                                                .StartsWith(p, StringComparison.OrdinalIgnoreCase) ?? false))
                                        && !(objectType[j].Attribute("Name")?.Value
                                                 .Contains("stop", StringComparison.OrdinalIgnoreCase) ??
                                             false)) //If the name doesn't start with anything command related nor contains stop, it's an IE
                                    {
                                        var circuitName = dptName;
                                        if (!string.IsNullOrEmpty(circuitName) &&
                                            !string.IsNullOrEmpty(prefix))
                                            circuitName = circuitName.Replace(prefix.Replace(" ", "_"), "");
                                        else
                                        {
                                            circuitName =
                                                string.Join("_",
                                                    circuitName.Split('_')[
                                                        1..]); //Takes the name of the object, except the first word 
                                        }

                                        var newType = int.Parse(objectType[j].Attribute("DPTs")?.Value
                                            .Split('-')[1] ?? "0"); //gets the type between the dashes in the xml
                                        var newAddress = objectType[j].Attribute("Address")?.Value!;
                                        var newDpt = new DataPointType(newType, newAddress, [],dptName);
                                        var modelIndex = FindSuffixInModels(circuitName, newFunctionalModels);
                                        if (modelIndex == -1)
                                        {
                                            lostDataPointTypes.Add(newDpt);
                                            continue;
                                        }
                                        for (var k = 0;
                                             k < newFunctionalModels[modelIndex].ElementList.Count;
                                             k++) //For each element, if for the same command 
                                        {
                                            var newElement = newFunctionalModels[modelIndex].ElementList[k];
                                            var name = string.Join("_",
                                                prefix.Replace(circuitName, "").Split('_')[1..]);
                                            if (name == "")
                                                name = string.Join("_",
                                                    prefix.Replace(circuitName, "").Split(' ')[1..]);
                                            var nbAppearances = newElement.CmdContains(circuitName);
                                            if (newType == 0 && nbAppearances != 0)
                                            {
                                                newType = newElement.GetDptType(name);
                                            }
                                            for (var l = 0; l < nbAppearances; l++)
                                            {
                                                newElement.AddDptToIe(newType, newAddress, []);
                                            }
                                        }
                                    }
                                }
                            }

                            index = functionalModelList.FunctionalModelDictionary.FunctionalModels.Count;


                            if (newFunctionalModels.Count > 0)
                            {
                                functionalModelList.AddToDictionary(
                                    new FunctionalModelStructure(newFunctionalModels[0], modelName, index + 1), true);
                                functionalModelList.FunctionalModels[index].Clear();

                            }

                        }
                    }

                    foreach (var newFunctionalModel in newFunctionalModels)
                    {

                        newFunctionalModel.UpdateValue();
                        functionalModelList.AddToList(index, newFunctionalModel, false);
                    }
                }

                if (lostDataPointTypes.Count > 0)
                {
                    functionalModelList.AddNewCount();
                    functionalModelList.AddToDictionary(new FunctionalModelStructure("Unrecognized DPTs"),false);
                    functionalModelList.FunctionalModels[^1].Clear();
                }

                foreach (var dpt in  lostDataPointTypes)
                {
                    functionalModelList.AddToList(functionalModelList.FunctionalModels.Count-1,new FunctionalModel(dpt.Name),false);
                    functionalModelList.FunctionalModels[^1][^1].AddElement(new TestedElement());
                    functionalModelList.FunctionalModels[^1][^1].ElementList[^1].AddDptToCmd(dpt);
                    functionalModelList.FunctionalModels[^1][^1].Name=dpt.Name;
                }
            }
            else// if (_groupAddressStructure == 2) //Case when Addresses are structured with 2 levels
            {
            }
            //functionalModelList.ExportList(@"C:\Users\manui\Documents\Stage 4A\Test\List");
        }
    }
    
    /// <summary>
    /// Determines the level structure of group addresses in an XML document to check for overlaps.
    /// 
    /// This method examines an XML document containing group address ranges and specific group addresses.
    /// It helps in identifying whether the group addresses are organized into 2 levels or 3 levels by detecting if there are any overlapping addresses.
    /// 
    /// If the addresses are detected to overlap, the method returns the value 3.
    /// If no overlaps are found, the method returns the value 2.
    /// 
    /// <param name="doc">The XML document (XDocument) containing the group address ranges and specific group addresses.</param>
    /// <returns>An integer indicating the overlap status: 3 for detected overlap, 2 for no overlap.</returns>
    /// </summary>
    public int DetermineGroupAddressStructure0Xml(XDocument doc)
    {
        // Ensemble pour vérifier les chevauchements d'adresses
        var allAddresses = new HashSet<int>();

        // Parcourir chaque GroupRange
        foreach (var groupRange in doc.Descendants(namespaceResolver.GlobalKnxNamespace! + "GroupRange"))
        {
            // Parcourir chaque GroupAddress du GroupRange
            foreach (var groupAddress in groupRange.Descendants(namespaceResolver.GlobalKnxNamespace! + "GroupAddress"))
            {
                var address = int.Parse(groupAddress.Attribute("Address")!.Value);

                // Si l'adresse est déjà dans l'ensemble, il y a chevauchement
                if (!allAddresses.Add(address))
                {
                    // Retourne 3 si un chevauchement est détecté
                    return 3;
                }
            }
        }

        // Si aucun chevauchement n'est trouvé, retourne 2.
        return 2;
    }
    
    /// <summary>
    /// Determines the level structure of group addresses in an XML document to check for overlaps.
    /// 
    /// This method examines an XML document containing group address ranges and specific group addresses.
    /// It helps in identifying whether the group addresses are organized into 2 levels or 3 levels by detecting if there are any overlapping addresses.
    /// 
    /// If the addresses are detected to overlap, the method returns the value 3.
    /// If no overlaps are found, the method returns the value 2.
    /// 
    /// <param name="modelStructures">The list of XElements containing the group address ranges and specific group addresses.</param>
    /// <returns>An integer indicating the overlap status: 3 for detected overlap, 2 for no overlap.</returns>
    /// </summary>
    public int DetermineGroupAddressStructureGroupAddressFile(IEnumerable<XElement>? modelStructures)
    {
        if (modelStructures == null)
            return 0;
        if (modelStructures.ToList()[0].Elements().ToList()[0].Elements().ToList().Count > 0)
            return 3;
       
        // if(modelStructures.ToList()[0].Attributes().ToList().Count == 0)
        // Si aucun chevauchement n'est trouvé, retourne 2.
        return 2;
    }
    
    /// <summary>
    /// Finds the biggest prefix corresponding to at least threshold (default 90 %) of the addresses
    /// </summary>
    /// <param name="strings">List of string where the prefix has to be found </param>
    /// <param name="threshold">Percentage of names that need to have the same prefix</param>
    /// <returns>The biggest prefix shared by at least the threshold</returns>
    string FindMajorityPrefix(List<string> strings, double threshold = 0.9)
    {
        if (strings.Count == 0)
            return string.Empty;

        int n = strings.Count;
        int minConsideredLength = strings
            .OrderByDescending(s => s.Length)
            .Take((int)Math.Ceiling(n * threshold)) // on ne garde que les plus longues qui forment la majorité
            .Min(s => s.Length);

        string bestPrefix = "";

        // On teste tous les préfixes jusqu'à la plus courte chaîne parmi la majorité
        for (int len = 1; len <= minConsideredLength; len++)
        {
            var prefixCounts = new Dictionary<string, int>();

            foreach (var s in strings)
            {
                if (s.Length < len) continue; // on ignore les bruits trop courts
                string prefix = s.Substring(0, len);
                if (!prefixCounts.ContainsKey(prefix))
                    prefixCounts[prefix] = 0;
                prefixCounts[prefix]++;
            }

            var bestAtThisLength = prefixCounts
                .OrderByDescending(kv => kv.Value)
                .First();

            // Vérifier si ce préfixe est majoritaire
            if (bestAtThisLength.Value >= n * threshold)
            {
                bestPrefix = bestAtThisLength.Key;
            }
            else
            {
                break; // dès que ça échoue, on arrête
            }
        }

        return bestPrefix;
    }
    
    /// <summary>
    /// Finds the biggest suffix corresponding to at least threshold (default 90 %) of the addresses
    /// </summary>
    /// <param name="strings">List of string where the suffix has to be found</param>
    /// <param name="threshold">Percentage of names that need to have the same suffix</param>
    /// <returns>The biggest suffix shared by at least the threshold</returns>
    string FindMajoritySuffix(List<string> strings, double threshold = 0.9)
    {
        // On inverse toutes les chaînes
        var reversedStrings = strings.Select(s => new string(s.Reverse().ToArray())).ToList();

        // On réutilise ta méthode pour trouver le préfixe majoritaire sur les chaînes inversées
        string reversedSuffix = FindMajorityPrefix(reversedStrings, threshold);

        // On inverse à nouveau pour obtenir le suffixe original
        return new string(reversedSuffix.Reverse().ToArray());
    }

    /// <summary>
    /// Finds the functional model with the name containing the string
    /// </summary>
    /// <param name="suffix"> String to find in the model name</param>
    /// <param name="models"> List of models where the suffix will be searched</param>
    /// <returns>The index of the model containing the suffix in the list </returns>
    public int FindSuffixInModels(string suffix, List<FunctionalModel> models)
    {
        if ( string.IsNullOrEmpty(suffix) || models.Count == 0) return -1;
        for (var i = 0; i < models.Count; i++) // If the suffix is directly in the model name give it
        {
            if (models[i].Name.Contains(suffix, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }
        for (var i = 0; i < models.Count; i++)//Otherwise check the name of all the DPTs if they contain it
        {
            foreach (var element in models[i].ElementList)
            {
                if (element.TestsCmd.Count <0 && element.TestsCmd[0].Name.Contains(suffix))
                    return i;
            }
        }
        return -1; //If not found, returns 0
    }

    /// <summary>
    /// Finds in a model the index of the element containing the right string
    /// </summary>
    /// <param name="stringToFind">String to find in the name of the dpt</param>
    /// <param name="model">The mdoel where the string has to be found</param>
    /// <returns></returns>
    public int FindStringInElement(string stringToFind, FunctionalModel model)
    {
        for (var i = 0; i < model.ElementList.Count; i++)
        {
            foreach (var dpt in model.ElementList[i].TestsCmd)
            {
                if (dpt.Name.Contains(stringToFind))
                {
                    return i;
                }
            }
        }
        return -1;
    }
}