#!/bin/bash

# Chemin vers le fichier app.xaml.cs
filePath="./KNX Virtual Integrator/KNX Virtual Integrator/app.xaml.cs"

# Lire tout le contenu du fichier ligne par ligne et stocker dans un tableau
mapfile -t fileContent < "$filePath"

# Initialiser une nouvelle variable pour stocker le contenu modifié
newContent=""

# Parcourir chaque ligne du fichier
for line in "${fileContent[@]}"; do
    # Utiliser une regex pour capturer l'indentation, la valeur actuelle et le texte après le nombre
    if [[ $line =~ ^(.*public\ const\ int\ AppBuild\ =\ )([0-9]+)(;.*)$ ]]; then
        # Extraire les parties de la ligne
        indentation="${BASH_REMATCH[1]}"
        currentValue="${BASH_REMATCH[2]}"
        suffix="${BASH_REMATCH[3]}"
        
        # Incrémenter la valeur actuelle
        newValue=$((currentValue + 1))
        
        # Construire la nouvelle ligne
        line="${indentation}${newValue}${suffix}"
    fi
    
    # Ajouter la ligne modifiée ou non à la nouvelle variable de contenu
    newContent+="$line"$'\n'
done

# Écrire le contenu modifié dans le fichier
echo "$newContent" > "$filePath"

echo "AppBuild version incremented in $filePath"
