# DOCUMENTATION – KNX Boost Desktop

Dernière révision : 08/09/2025 (KNX Boost Desktop v1.3)

## Langues pour la documentation / Documentation language :
- [Français](French_Documentation.md)
- [English](English_Documentation.md)

## Table des matières
1. 🖥 [Installation](#installation-title)

   1.1. 📥 [Téléchargement](#downloading)

   1.2. 💻 [Déroulement de l’installation](#installing)

2. 🔍 [Aperçu de l’application](#overview-title)

   2.1. 🪟 [Fenêtre principale](#main-window)

   2.2. ⚙️ [Menu paramètres](#settings-window)

   2.3. 🪟 [Fenêtre de connection](#Connection-window)

   2.4. 🪟 [Fenêtre d'édition des structures](#structure-window)

   2.5. 🪟 [Fenêtre d'analyse](#analysis-window)

   2.6. 🪟 [Fenêtre de rapport d'analyse](#report-window)

3. 🛠 [Utilisation de l’application](#user-title)

   3.1. ⚙️ [Modifier les paramètres](#modify-settings)

   3.2. 📥 [Importation depuis ETS](#ets-import)

   3.3. 🪟️ [Connexion au bus KNX](#bus-connection)

   3.4. 📝 [Création d'un test](#create-test)

   3.5. 🪟 [Lancement d'un test](#launch-test)

   3.6. 🪟 [Création du rapport](#create-report)

   3.7. 📤 [Import/Export de projets KNX VI](#vi-import)

4. 🆘 [FAQ](#faq-title)


[← Retour](../README.md)

<br></br>
# 1. Installation <a name="installation-title"></a>
## 1.1 Téléchargement <a name="downloading"></a>

Pour installer l’application KNX Virtual Integrator, téléchargez l’installateur [KNX_VI-Installer_vX.X.exe](https://github.com/noecail/UCRM-KNXVirtualIntegrator_2025/releases) de la dernière version stable du logiciel dans les releases de ce repository GitHub.
La dernière version est identifiée par "Latest" et est souvent la plus haute dans la liste.

## 1.2 Déroulement de l'installation <a name="installing"></a>

Pour installer et lancer l’application, veuillez suivre les indications suivantes :

1. **Lancez l’installateur**

   Double-cliquez sur le fichier `KNX_VI-Installer_vX.X.exe` pour lancer l’assistant d’installation.

2. **Passez le message “Windows a protégé votre ordinateur”**

   Lorsque le message “Windows a protégé votre ordinateur” s’affiche :

   * Cliquez sur **"Informations complémentaires"**.
   * Cliquez ensuite sur **"Exécuter quand même"**.<br>

   > **Note :** **_Cela ne signifie pas que l’application est dangereuse._** Ce message est généré par **Microsoft Defender SmartScreen**, un composant de sécurité intégré à Windows. Il s’affiche lorsque vous essayez d’exécuter une application téléchargée depuis Internet qui n’est pas encore reconnue par Microsoft. Cela ne signifie pas que l’application est dangereuse, mais simplement qu’elle n’a pas encore été largement téléchargée et vérifiée par Microsoft.

3. **Autorisez les modifications**

   Si le système affiche une demande de contrôle de compte d’utilisateur (UAC), cliquez sur **"Oui"** pour autoriser l’application à apporter des modifications à votre appareil.

4. **Sélectionnez la langue**

   Choisissez la langue de l’installation dans le menu déroulant et cliquez sur **"OK"**.

5. **Selectionnez l'emplacement de l'application**

   L'installateur va demander où mettre les documments de l'application.
   La modification de ce dossier d'application peut entrainer la suppression ou modification de documents importants de l'ordinateur puisque l'application supprime tout document autre que les siens dans le dossier.
   Il est donc déconseillé de modifier les dossiers par défaut.
   Cliquez sur **Suivant** puis **Suivant**. 

6. **Créer une icône sur le bureau**

   Cochez la case **"Créer une icône sur le Bureau"** si vous voulez créer une icône de l’application KNX Virtual Integrator sur votre Bureau. Cliquez sur **"Suivant"** pour continuer.

7. **Prêt à installer**

   Une fenêtre récapitulative s’affiche, cliquez sur **"Installer"** pour commencer l’installation.

8. **Installation terminée**

   Une fois l’installation terminée, vous verrez une fenêtre de confirmation. Cochez la case **"Exécuter KNX Boost Desktop"** si vous souhaitez démarrer l’application immédiatement, puis cliquez sur **"Terminer"**.



<br></br>
# 2. Aperçu de l'application <a name="overview-title"></a>
## 2.1. 🪟 Fenêtre principale <a name="main-window"></a>

La fenêtre principale est composée de 5 parties principales :

<img src="Images/MainWindow.png" alt="fenetre-principale" style="width:50%;"/>

**1. Le bandeau supérieur :**

Dans ce bandeau, vous pouvez :
- ⚙️ [Modifier les paramètres](#modify-settings) de l'application en appuyant sur le bouton ⚙️.
- 📥 [Importer des adresses de groupe KNX](#ets-import) dans l’application en cliquant sur le bouton “**Importer des adresses**”.
- 📥 [Importer un projet ETS](#ets-import) dans l’application en cliquant sur le bouton “**Importer un projet**”.
- 📥 [Lancer une analyse de l'installation](#launch-test) en ouvrant la [fenêtre d'analyse](#analysis-window) avec le bouton "**Paramètres de test**".
- 📤 [Exporter le rapport d'analyse](#create-report) en ouvrant la [fenêtre de rapport](#report-window) avec le bouton "**Exporter le rapport**".

**2. Panneau des adresses originales :**

C’est dans ce panneau que, une fois votre projet importé, les adresses de groupes telles qu’elles sont nommées dans le projet actuellement apparaîtront.

**3. Panneau des adresses corrigées :**

De la même manière, c’est dans ce panneau que les adresses de groupe corrigées apparaîtront. Voir la partie 🚶‍♂️‍➡️ [Naviguer dans le projet modifié](../UtilisationApplication/naviguer-dans-le-projet-modifie.md) pour plus de détails sur ces vues.

## 2.2. Menu paramètres <a name="settings-window"></a>

## 2.3. Fenêtre de connexion <a name="connection-window"></a>

## 2.4. Fenêtre d'édition des structures <a name="structure-window"></a>

## 2.5. Fenêtre d'analyse <a name="analysis-window"></a>

## 2.6. Fenêtre de rapport d'analyse <a name="report-window"></a>


<br></br>
# 3. Utilisation de l'application <a name="user-title"></a>
## 3.1. Modifier les paramêtres <a name="modify-settings"></a>

## 3.2. Importation depuis ETS <a name="ets-import"></a>

## 3.3. Connexion au bus KNX <a name="bus-connection"></a>

## 3.4. Création d'un test <a name="create-test"></a>

## 3.5. Lancement d'un test <a name="launch-test"></a>

## 3.6. Création du rapport <a name="create-report"></a>

## 3.7. Import/Export de projets KNX VI <a name="vi-import"></a>


<br></br>
# 4. FAQ <a name="faq-title"></a>

**Pourquoi n'est-il pas possible d'installer l'application pour tous les utilisateurs en même temps?**</br>
Probablement à cause de problèmes de dossiers, d'autorisations et de clés de registres, 
l'application peut ne jamais réussir à se lancer lorsqu'elle est installée autre part que dans les AppData de l'utilisateur.

<br></br>
[← Retour](../README.md)
