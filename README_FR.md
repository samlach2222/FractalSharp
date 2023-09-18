# FractalSharp

[![Version du projet](https://img.shields.io/badge/version-1.0-purple.svg)](https://img.shields.io/badge "Version du projet")

<p align="center">
  <img src="Project Repport/FractalSharp logo.svg" width="40%">
</p>

## Menu
- [Introduction](#introduction)
- [Performance](#performance)
- [Compilation et démarrage](#compilation-et-démarrage)

## Introduction
### En quoi consiste FractalSharp
Le projet de système distribué FractalSharp consiste en l’implémentation de la suite de Mandelbrot
en utilisant un système distribué. Il peut être exécuté de plusieurs manières
- Sur 1 seul processus
- Sur une multitude de processus
- Sur plusieurs machines interconnectées

Une fois l’image de la fractale de Mandelbrot calculée, elle est alors affichée à l’écran et l’utilisateur peut
dessiner un rectangle (gardant le même ratio que l’image de base) pour recalculer de la même manière la
suite de Mandelbrot, générer l’image zoomée et l’afficher.

### Choix des technologies
La partie système distribué de FractalSharp est réalisée à l’aide d’MPI (Message Passing Interface).
De plus, FractalSharp est réalisé avec deux langages de programmation différents :
- **FractalSharp** et **FractSharpMPI** sont deux programmes réalisés en C# pour Windows, pour faire
fonctionner MPI, nous avons utilisé MPI.NET de Microsoft. FractalSharp est conçue et adaptée pour
Windows, cependant nous avons besoin d’exécuter notre programme sur des ordinateurs Linux afin
d’utiliser le cluster qui nous est mis à disposition. Hors MPI.NET est très complexe à compiler sur linux
et dotnet est absent sur les ordinateurs mis à disposition. Il existe donc :
- **FractalPlusPlus** et **FractalPlusPlusMPI** sont deux programmes réalisés en C++ et sont principalement
prévus pour Linux, pour fonctionner sur le cluster mis à disposition

### Découpage du problème
La problématique peut facilement être découpée à partir de deux points cruciaux :
- L’image générée par le calcul de la suite de Mandelbrot doit être réalisé par **un programme à part**
(Les version ...MPI des programmes) car on ne peut pas relancer de calcul MPI à partir d’un programme
qui viens de finir son calcul MPI (Les processus MPI finissent le programme main et ne peuvent donc
pas être recréés).
- L’affichage de l’image et la partie qui demande le calcul de la suite de Mandelbrot au programme MPI
doivent tout deux être dans **un Thread différent** car l’attente des clics souris pour dessiner le
rectangle du zoom sur l’image serait bloquant pour le calcul MPI.

## Performance
### Comparaison des performances
L’enjeu de notre programme, c’est de proposer une analyse des performances sur l’ajout de MPI
dans le calcul d’une fractale basée sur la suite de Mandelbrot. Nous avons 3 conditions de tests différents :
- C# (Windows)
- C++ (Windows)
- C++ (Linux)

Sur ces 3 conditions, nous allons lancer 6 tests différents :
- Image en 640x630 non zoomée
- Image en 640x360 zoomée
- Image en 1280x720 non zoomée
- Image en 1280x720 zoomée
- Image en 4K (3840x2160) non zoomée
- Image en 4K (3840x2160) zoomée
- Image en très haute qualité (16000x9000) non zoomée

Sur ces 5 premiers tests, nous avons pris les nombres de processus suivants :
- C# - de 1 à 8
- C++ Windows – de 1 à 8
- C++ Linux – de 1 à 8 puis 12, 16, 32 et 64

Sur la dernière image seuls les tests C++ Linux ont été effectués.
<p align="center">
  <img src="https://github.com/samlach2222/FractalSharp/assets/44367571/b4a40d54-3d50-45fc-b00e-9c67207f7b94" width="40%" margin-left="1%">
  <img src="https://github.com/samlach2222/FractalSharp/assets/44367571/c3d68672-cffd-4d7a-86f9-4d39bf151866" width="40%" margin-right="1%">
</p>

<p align="center">
  <img src="https://github.com/samlach2222/FractalSharp/assets/44367571/3c909da1-c57c-4953-9add-638b7e8a3e84" width="75%">
</p>

Sur ce premier test, nous avons une image en 640x630 non zoomée. Les performances sur Windows
ne vont être testée que jusqu’à 8 processus car pas de possibilité dans notre configuration actuelle de tester
sur plus.

Les performances de C# et C++ Linux sont de plus en plus mauvaise par rapport à la hausse du nombre de
processus car l’image étant très petite, le temps de déplacement des données est supérieur au temps gagné
en parallélisant les calculs. On obtient de très bonnes performances sur la version C++ Windows.

<p align="center">
  <img src="https://github.com/samlach2222/FractalSharp/assets/44367571/94c40791-028c-4be7-8ca2-6fd0b36a6e4d" width="75%">
</p>

Sur ce second test, nous avons une image en 640x630 zoomée. Sur une petite image, les performances
zoomées sont sensiblement les mêmes que sur une image dézoomée.

<p align="center">
  <img src="https://github.com/samlach2222/FractalSharp/assets/44367571/72424914-db25-4875-9703-d16f70b6d383" width="75%">
</p>

Sur ce troisième test, nous avons une image en **1280x720** non zoomée. Sur cette image de taille
moyenne, nous commençons à avoir des différences significatives. La performance du programme C# sur
Windows se dégrade très rapidement. Cependant, sur le même système d’exploitation, C++ obtient des
performances légèrement meilleures en augmentant le nombre de processus MPI. Cependant le gain de
performance ne justifie pas l’utilisation d’MPI et de 8 processus différents. L’autre grande différence se fait sur
Linux avec C++. Jusqu’ici les performances étaient plutôt constantes jusqu’à 16 processus, mais se dégradaient
énormément au-delà. Sur cette image les performances, comme sur Windows sont meilleures jusqu’à 16
processus, se dégradent au-delà, mais très nettement moins. Cela permet d’émettre l’hypothèse que les
performances au-delà de 16 processus seraient très bonnes sur des grandes images.

<p align="center">
  <img src="https://github.com/samlach2222/FractalSharp/assets/44367571/2443d467-a760-4343-a1a9-dcb4e27dfc53" width="75%">
</p>

Sur ce quatrième test, nous avons une image en **1280x720** zoomée. Il n’y a pas de changement
significatif entre l’image zoomée et l’image non zoomée

<p align="center">
  <img src="https://github.com/samlach2222/FractalSharp/assets/44367571/25d9c12e-fe5f-4f04-9439-6fc9834b817e" width="75%">
</p>

Sur ce cinquième test, nous avons une image en **4K** non zoomée. A partir d’ici, nous n’allons plus nous
intéresser aux performances de C#, mais les performances C++ commencent à être de plus en plus
intéressantes.

Les temps en secondes commencent à être de plus en plus grands, et les performances Windows et Linux de
plus en plus proches. L’hypothèse précédente commence à se confirmer, Les performances sont maintenant
mauvaises avec peu de processus MPI, et deviennent largement meilleur plus on rajoute de processus. Il n’y
maintenant que très peu de différences au-delà de 16 processus.

<p align="center">
  <img src="https://github.com/samlach2222/FractalSharp/assets/44367571/97e94492-aca1-49cf-acfe-715c4f6685ed" width="75%">
</p>

Sur ce sixième test, nous avons une image en **4K** zoomée. Il n’y a que peu de changement par rapport à
l’image non zoomée, cependant la différence de performance entre C++ Windows et Linux s’efface encore un
peu plus.

<p align="center">
  <img src="https://github.com/samlach2222/FractalSharp/assets/44367571/3bc0f840-cd52-499a-a205-2a366846212b" width="75%">
</p>

Sur ce septième et dernier test, nous avons une image en **16000x9000** zoomée. Sur cette image, nous
avons voulu démontrer notre hypothèse. Nous avons donc uniquement recueilli les données C++ Linux.
Sur cette très grande image, on observe une très nette différence entre l’usage de MPI et quand on ne l’utilise
pas. On observe également de très bonnes performances quand on utilise 64 processus (plus on utilise de
processus, plus les performances sont élevées).


## Compilation et démarrage
### Procédure de compilation
Pour lancer le programme, nous allons tout d’abord procéder à sa compilation. Vu que FractalSharp
et FractalPlusPlus sont deux programmes différents, nous allons détailler les étapes de compilation de chacun.
1. Se rendre dans le dossier Code source de l’archive, ou (optionnelle) cloner depuis git [https://github.com/samlach2222/FractalSharp.git](https://github.com/samlach2222/FractalSharp.git)
2. Se rendre dans le dossier FractalSharp pour le projet C# et FractalPlusPlus pour le projet C++

**FractalSharp (Windows)**
1. Lancer le fichier batch pour installer MPI et SDL : `REQUIREMENTS/Install_SDL.bat`
2. Installez Visual Studio, puis lancez le projet avec le fichier `FractalSharp.sln`
3. Effectuer un clic droit sur la solution puis `Générer la solution`
4. Rendez-vous dans le dossier `.\FractalSharp\bin\[Release|Debug]\net6.0-windows\`

**FractalPlusPlus (Windows)**
1. Lancer le fichier batch pour installer MPI et SDL : `REQUIREMENTS/Install_SDL.bat`
2. Installez Visual Studio, puis lancez le projet avec le fichier `FractalPlusPlus.sln`
3. Effectuer un clic droit sur la solution puis `Générer la solution`
4. Rendez-vous dans le dossier `.\x64\[Release|Debug]\`

**FractalPlusPlus (Linux)**
1. Exécuter le programme d’installation avec la commande `./build_linux.sh`
2. Rendez-vous dans le dossier `.\build_linux\`

### Démarrage + Données de tests
Nous allons maintenant pouvoir lancer le programme. Sur chaque version, nous avons deux manières de lancer le
programme. La première est la plus classique, c’est à dire, lancer le programme GUI (avec l’affichage
graphique). Sur celui-ci, vous pouvez zoomer en dessinant un rectangle. La deuxième manière de lancer le
programme est d’utiliser uniquement la partie MPI auquel cas le lancement se fait avec des arguments en
ligne de commande.

**Sur Windows :**

GUI → Lancer le programme **FractalSharp.exe** ou bien **FractalPlusPlusGUI.exe** (en fonction du langage de
programmation souhaité.

MPI → Lancer le programme FractalSharpMPI.exe ou bien FractalPlusPlusMPI.exe (en fonction du langage
de programmation souhaité) de la manière suivante :

`mpiexec -n [NombreProcessusMPI] [FractalSharpMPI.exe | FractalPlusPlusMPI.exe] [TailleX] [TailleY] [minComplexX] [maxComplexX]
[minComplexY] [maxComplexY]`

**Sur Linux :**

GUI → Lancer le programme **./FractalPlusPlusGUI**

MPI → Lancer le programme **./FractalPlusPlusMPI** de la manière suivante 

`mpiexec -hostfile [NomFichierHost] -n [NombreProcessusMPI] ./FractalPlusPlusMPI [TailleX] [TailleY] [minComplexX] [maxComplexX]
[minComplexY] [maxComplexY]`

**Données de tests :**

Pour la version GUI, il n’y a pas vraiment de données de tests, cette version sert surtout pour vérifier le bon
fonctionnement. Libre à vous de zoomer à votre convenance. Cependant, à partir d’un moment (entre le 3ème
et 4ème zoom), le programme affiche que du noir, c’est parce que nous atteignons le nombre de décimales
maximal pour notre algorithme de calcul.

Pour la version MPI, Les données de tests recommandées sont les suivantes :

- Calcul d’une image en **FullHD non zoomée** : `TailleX = 1920 TailleY = 1080 minComplexX = -2 maxComplexX = 2
minComplexY = -1.125 maxComplexY = 1.125`
- Calcul d’une image en **FullHD zoomée** : `TailleX = 1920 TailleY = 1080 minComplexX = -1.828 maxComplexX = -1.64
minComplexY = -0.057 maxComplexY = 0.049`
- Calcul d’une image en **16000*9000 non zoomée** : `TailleX = 16000 TailleY = 9000 minComplexX = -2 maxComplexX = 2
minComplexY = -1.125 maxComplexY = 1.125`
