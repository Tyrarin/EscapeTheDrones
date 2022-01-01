# EscapeTheDrones


## Description du scénario

Le joueur incarne un personnage, une capsule rouge. Le but est de se déplacer jusqu'à trouver un disque vert : une fois que le joueur est parvenu à aller dans ce disque,
il a gagné. Ceci dit, il n'est pas évident de le repérer car la caméra (en vue à la première personne) est assez basse et ne permet donc pas de le distinguer de loin.
De plus, la position du joueur mais aussi du disque vert sont aléatoires sur une carte de 130m x 130m (1,7 hectare).

Cependant la vraie difficulté réside en la surveillance par plusieurs drones sur la carte : si l'un d'eux détecte le joueur, il va avertir une équipe de 3 robots snipers
qui vont tirer sur le joueur toutes les 2 secondes tout en se déplaçant vers lui. Les dégats sont inversement proportionnels à la distance entre un robot et le joueur et ce dernier n'a que 300 points de vie ; il ne faut donc pas laisser les robots se rapprocher, ou alors il faut se cacher derrière l'un des 30 murs placés aléatoirement sur la carte.

Dès lors que le joueur est détecté par au moins un drone, il peut alterner entre la vue à la première personne et une vue globale aérienne en appuyant sur la touche 'C'.
Cela dit en vue aérienne, puisqu'il est facile de situer le cercle vert, le joueur est 2,5 fois plus lent qu'en vue à la première personne.

Il est d'ailleurs possible de modifier la vitesse du joueur grâce à des bonus eux aussi placés aléatoirement. Ces bonus de vitesse prennent la forme de sphères bleues et augmentent de
50% la vitesse du joueur et sont cumulables.
Il existe aussi des sphères noires, plus rares, qui permettent de stopper tous les drones et robots actifs pendant une durée de 5 secondes.

Dès qu'un drone ne patrouille plus il soumet aux drones restants une nouvelle répartition des zones de surveillances de telle sorte à ce que toute position est susceptible d'être surveillée par un des drones.

![cercle_vert](./Images/cercle_vert.JPG)

Le disque vert à atteindre pour remporter la partie

![drone](./Images/drone.JPG)

Un des drones qui surveille sa zone en volant à une altitude de plusieurs mètres

![robots](./Images/robots_squad.JPG)

Une des équipes de robots attendant patiemment d'être réveillée


![victoire](./Images/demo2.gif)

Durant cette partie, le joueur récupère deux sphères bleues pour augmenter sa vitesse et donc ses chances de trouver rapidement le disque vert. En parcourant la carte, il se fait détecter par le drone 1 (en bleu). Pendant un temps le joueur choisit d'ignorer la menace que représentent les robots qui ont été réveillés. Cela dit, au bout d'une quinzaine de secondes il se rend compte que sa jauge de vie descend à vue d'oeil et décide de presser la touche 'C' pour passer en vue aérienne et assure la victoire en consommant une sphère noire, stoppant dans leur élan tous les drones et robots actifs pendant une durée de 5 secondes.

![defaite](./Images/demo3.gif)

Dans cette partie le joueur se fait détecter par plusieurs drones et choisit de ne pas passer en vue aérienne pour gagner dans les règles de l'art. Cependant, voyant sa jauge de vie se vider, il décide au final de sortir de la vue à la première personne pour espérer s'en sortir mais c'est peine perdue : le disque vert est trop éloigné et les 6 robots mettent très vite le joueur hors d'état de nuire.

## Implémentation du projet

### Arborescence des dossiers

Le dépot est composé de plusieurs dossiers différents :
- `Assets/` qui est le dossier comprenant la quasi-totalité du projet. C'est dans ce dossier que se trouvent tous les scripts `C#`, les prefabs (contenant des prefabs pour les murs, les robots ou encore les drones), les materials ou encore le contenu de la scène correspondant au scénario.
- `Build/` qui contient les fichiers nécessaires à l'exécution du scénario dont le fichier exécutable `.exe`.
- `Libary/` qui est une sorte de cache pour `Assets/`.
- `Logs/` qui comprend toutes les traces de messages, de warnings ou d'erreurs apparus dans l'application.
- `UserSettings/` qui contient les préférences de l'utilisateur dans l'éditeur.
- `Packages/` où se trouvent une multitude d'éléments compressés du projet.
- `ProjectSettings/` contenant plein de configurations du projet.

### Les scripts

Pour faire ce projet, trois scripts ont été nécessaires :
- Player.cs qui va principalement permettre au joueur de se déplacer, mais qui va également gérer les intéractions avec plusieurs objets comme le disque vert ou encore les bonus bleus et noirs.
- Drone.cs qui va gérer les différents comportements de chaque drone : la délimitation de sa zone de patrouille,
son parcours qui est aléatoire au sein de sa zone mais également la manière d'envoyer des raycasts et d'agir en fonction de ce qu'il détecte. Chaque drone est doté d'une liste de tous les drones pour communiquer facilement.
- Robot.cs qui gère le comportement des robots lorsqu'ils doivent infliger des dégats au joueur. Ils doivent notamment tenir compte d'éventuels murs entre eux et le joueur, auquel cas ils ne peuvent pas tirer.

### Déplacement du joueur

En vue à la première personne, il faut appuyer sur la touche 'W' pour avancer. Pour cela il était difficile de se contenter de `Input.GetKeyDown(KeyCode.W)` ou `Input.GetKeyUp(KeyCode.W)` car il fallait alors appuyer de nombreuses fois pour avancer un petit peu, en raison la fréquence de l'appel à `Update()`. J'ai alors eu l'idée de passer un booléen à `true` lorsque `Input.GetKeyDown(KeyCode.W)` vaut `true`, et d'effectuer le déplacement à chaque itération tant que le booléan est vrai avec `this.transform.position += this.transform.forward * this.speed/10 * Time.deltaTime`. Dès que `Input.GetKeyUp(KeyCode.W)` est à `true`, cela signifie que l'utilisateur a lâché la pression sur le bouton 'W' et le booléen est alors passé à `false`, arrêtant ainsi le mouvement.
Pour ce qui est de la caméra, le déplacement horizontal de la souris est mesuré avec `Input.GetAxis("Mouse X")`. La rotation sur l'axe y est alors décrémentée du produit de cette mesure de la souris par un facteur de vitesse déterminé dans les attributs de la classe. Il suffit alors d'effectuer la rotation de la caméra en attribuant à la `transform.localRotation` la valeur de `Quaternion.Euler(0f, -1f * rotationY, 0f)`.

En vue aérienne, le principe de l'appui des touches est également utilisé sauf qu'ici ce n'est plus la touche 'W' qui produit un déplacement. Puisque la vue est globale (et donc éloignée du sol), il est très dur voire impossible de repérer l'avant ou l'arrière du personnage. De ce fait, le déplacement est géré par les flèches haut, bas, gauche et droite pour déplacer le joueur respectivement vers l'avant avec `Vector3.forward`, l'arrière avec `Vector3.back`, la gauche avec `Vector3.left` et la droite avec `Vector3.right`. La caméra est quant à elle fixée à une certaine hauteur telle qu'elle peut voir l'entièreté de la carte en étant orientée à 90 degrés sur l'axe des abscisses.

C'est la même caméra qui est utilisée pour les deux types de vues, elle voit simplement sa position être modifiée en conséquence.

### Déplacement des drones et détection via raycasts

Les drones se voient attribués (dans `Start()`) une zone qu'ils vont devoir surveiller. Cette attribution est faite dynamiquement dans le code.
Chacune des zones de patrouille est un rectangle de la longueur de la carte et avec un largeur étant le quotient de la largeur de la carte par le nombre de drones patrouilleurs. 

Aussi, chaque drone est placé aléatoirement dans sa zone de patrouille.

Toutes les 5 secondes, chaque drone va effectuer une rotation sur l'axe Y grâce à l'appel de la méthode `RotationDroneRandom()`. Cette valeur de rotation peut prendre les valeurs 0, 90, 180 ou 270. Ainsi il peut faire une rotation dans n'importe quelle direction ou peut continuer dans la même qu'avant la rotation.
Puisque les zones sont assez étroites en comparaison à la longueur, la direction de cette dernière est privilégiée : si le drone se déplace dans la direction de la longueur (quel que soit le sens), c'est-à-dire l'axe Z, il a plus de chances de continuer tout droit. Spécifiquement, un drone se déplaçant dans cette direction a 50% de probabilité de ne pas continuer tout droit.
Aussi, la méthode `Mathf.Clamp` est utilisée pour garder un drone dans sa zone : s'il se rapproche un peu trop d'une des limites à telle point qu'il se retrouve à une distance inférieure à 2 mètres, il est remis à sa place et effectue une nouvelle rotation aléatoire.
Ainsi, en jeu, il est possible de conjecturer leur future trajectoire en pronostiquant s'ils vont continuer d'aller tout droit ou non, en sachant qu'il y a toujours un risque qu'au dernier moment ils changent totalement de direction (une chance sur deux).

A chaque itération de `Drone.Update()`, la méthode `DetectionLaser()` est appelée. Celle-ci permet au drone de lancer une multitude de raycasts au moyen de 2 boucles `for`. L'intersection entre ces rayons et le sol produit un carré de plusieurs mètres de côté. 

![detection](./Images/detection.JPG)

Ces rayons sont particulièrement envoyés sur la couche 8, celle sur laquelle se trouve le joueur. Ainsi, si `Physics.Raycast(ray, out hit, range, layer_mask)` vaut `true`, cela signifie qu'un rayon est entré en contact avec le joueur et que ce dernier se trouve alors juste en-dessous du drone. 
Ce drone a alors 2 missions : la première est de prévenir les autres drones qu'il a détecté le joueur et qu'il ne peut plus patrouiller : les autres drones vont alors se partager la zone laissée à l'abandon. Plus spécifiquement toutes les zones vont être recalculées (par le drone qui se retire) pour être partagées équitablement entre tous les drones restants.
La deuxième mission du drone consiste à aller jusqu'à un point se trouvant au-dessus de son équipe de robots appelé le HQ (quartier général) avant de réveiller les dits-robots. Sur les axes X et Z, le HQ est placé à la position moyenne des robots de l'équipe et à la même altitude que le drone.
Ces deux missions font respectivement appel à deux méthodes très importantes de la classe Drone :
- `AdvancedRepartition()` : Elle est appelée dès la détection du joueur par un drone. Grâce à une liste contenant tous les drones (dont le drone qui appelle `AdvancedRepartition()`), le drone se retire de sa liste de drones ainsi que de celle des autres drones puis appelle `InitZoneLimit` qui va recalculer les zones des drones restants.
Contrairement à une version antérieure (trouvable dans des commits antérieurs dans le dépot sous le nom de `IntelligentRepartition()`), cette modification des zones est dynamique car elle ne dépend pas du nombre de drones. Pour ajouter un drone patrouilleur, il suffit simplement de créer un nouveau drone et de lui attribuer un nouveau tag (lui mettre un tag déjà existant est une mauvaise idée car le drone au tag doublon pourrait bien s'attribuer des robots qui ne sont pas censés être sous sa responsabilité). Si on veut que le drone fasse quelque chose après avoir détecté le joueur, il est important de lui confier également un ou plusieurs robots ayant le même tag.
- `AwakeRobots()` : Elle est appelée une fois que le drone a rejoint le quartier général. Ce dernier va alors réveiller tous les robots de la liste de son équipe en passant l'attribut `isActive` de chacun de ces robots à `true`. Il va aussi leur confier les deux masques à prendre en compte pour leur mission : `layer_mask` qui va permettre de lancer des raycasts dans la couche où se trouve le joueur, et `layer_mask_wall` qui est le masque permettant aux robots de détecter les murs. Une fois que le drone a appelé `AwakeRobots()`, il reste au même point à altitude constante.

### Déplacement des robots et mise à feu via raycasts

A chaque itération de `Robot.Update()`, si l'attribut `isActive` est à `true`, un robot avance vers le joueur grâce à la méthode `MoveTowards`. Un robot a la capacité de lancer un unique rayon directement sur le joueur tant qu'il a un ligne de vue. Ainsi, si un mur se trouve entre le robot et le joueur, le robot ne pourra pas infliger des dégâts au joueur. Dans l'implémentation, le robot tire un raycast à la fois sur la couche du joueur et sur la couche du robot. Si le booléen renvoyé par le raycast du joueur est à `true` mais que ce n'est pas le cas du booléen renvoyé par le raycast dans la couche du mur, alors le robot peut infliger les dégâts. Si les deux booléens sont à `true`, le robot calcule alors la distance entre le mur et lui puis celle entre le joueur et lui : si la première distance est plus grande que la deuxième, cela signifie que le mur est derrière le joueur et qu'il peut donc tirer. Sinon, cela signifie que le mur bloque la ligne de vue et que le robot ne peut alors pas tirer.
Après chaque tir de rayon, un timer de 2 secondes est lancé pour rendre le jeu un tant soit peu jouable.

Pour ce qui est des dégâts infligés par les robots, ils sont inversement proportionnels à la distance entre le joueur et les robots. En partant d'une base de 600 HPs en dégats (le joueur n'a que 300 HPs), il faut multiplier cette base par l'inverse de la distance. Ainsi si la distance est notée `d`, les dégâts totaux sont de `600/d`.*
Par conséquent le montant minimal de dégats infligeables par un robot peut être obtenu en plaçant un robot et le joueur dans deux coins opposés de la carte. Avec une carte de 130 mètres de côtés, la longueur de la diagonale vaut `sqrt(130² + 130²)` soit 183,85 mètres. `Puisque 600 / 183,85` vaut 3,26 et que l'on prend la valeur entière pour les dégâts totaux, un robot peut infliger au minimum 3 points de vie de dommage au joueur.

Quant à la quantité maximale de dégâts qu'un robot peut infliger, il faut considérer la distance minimale possible.
Un robot et le joueur étant des objets rigides, il est impossible qu'ils soient exactement à la même position (évitant ainsi une division par 0 dans le calcul des dégâts). La distance minimale est alors la somme des moitiés des diamètres du robot et du joueur qui peuvent être vus comme des cylindres. Ainsi `d = 1/2 + 1/2 = 1` mètre.
De ce fait un robot peut infliger au maximum `600 * 1/1 = 600` de dégâts au joueur, soit deux fois plus que le nombre de points de vie qu'il possède.

## Présentation d'un concept d'Unity : le canvas

Avec Unity, l'interface utilisateur (UI) est synonyme de canvas. C'est un rectangle qu'il est possible d'adapter à l'écran de d'appareil lançant la simulation et sur lequel il est possible d'ajouter des éléments de l'UI en tant que fils. Tous ces éléments sont automatiquement placés dans une couche appelée `"UI"`.
A même le canvas, l'élément le plus fréquemment ajouté est le panel. Cela dit, dans ce projet, la jauge de vie du joueur et le message indiquant qu'un bonus a été consommé viennent eux-aussi se placer directement sur le canvas.

S'il est coutume de ne placer qu'un panel par canvas et de créer un canvas par scène puis de faire en sorte de changer de scène par l'appui d'un bouton, ce n'est pas ce qui a été fait dans ce projet.
En effet, il n'y a ici qu'un seul et unique canvas accueillant pas moins de 4 panels différents.
Pour n'en afficher qu'un à la fois, la méthode `GameObject.SetActive` est beaucoup utilisée. Par rapport à l'utilisation de plusieurs scènes, cette méthode a l'avantage de ne pas nécessiter l'utilisation fréquente de la méthode `DontDestroyOnLoad` sur les objets de la scène principale.

Dans EscapeTheDrones, le panel actif au début de la partie est `Start`, sur lequel la présentation du scénario ainsi que les règles sont inscrites. Dès que la touche `Esc` est pressée, le panel est rendu inactif alors que la jauge de vie `Slider` et le panel `Log` sont eux rendus actifs. `Log` est un petit panel qui vient se mettre dans le coin inférieur droit de l'écran de l'utilisateur. Les communications entre les drones y sont affichées et l'utilisateur est également notifié (après une détection) qu'il peut passer en vue aérienne s'il le souhaite. Si les robots parviennent à disposer du joueur, le drone qui commande les robots l'ayant vaincu se vante de sa réussite dans `Log` avant que le panel `GameOver` soit rendu actif après quelques secondes. Ce menu sert d'une part à notifier l'utilisateur de sa défaite, mais aussi à lui proposer de rententer sa chance en appuyant sur la touche 'G'.
Dans le cas où le joueur a réussi à atteindre le disque vert avant de se faire éliminer par les robots, c'est le panel `Success` qui devient actif et qui, à l'instar du panel `GameOver` notifie l'utilsateur de sa victoire et lui propose de rejouer en pressant là encore 'G'.

## Problèmes d'implémentation et bugs

Lorsque le joueur consomme un bonus en entrant en contact avec une sphère bleue ou une sphère noire, une exception est lancée mettant en garde le fait de modifier des collections. Ceci-dit les sphères fonctionnent tout de même parfaitement et cela ne pose pas de problème pour l'exécutable.

Un autre soucis concerne justement la sphère bleue permettant d'augmenter la vitesse du joueur. 
Lorsque le joueur atteint une vitesse particulièrement élevée (à partir de 2 sphères bleues consommées), il passe à travers les murs. Ce problème persiste même en ajoutant un composant `RigidBody` à chaque mur et en leur attribuant des masses de plusieurs tonnes. Disons qu'il est si rapide qu'il parvient à phaser à travers la matière.


Nota Bene : l'exécutable porte le nom de `Turn-based fight.exe` car c'était à l'origine le nom du projet avant que le concept d'EscapeTheDrones ne soit trouvé. 

## Sources

- https://github.com/tdeporte/Unity_FPS : inspiration du dépot git de Tom et Axel pour mettre en place dans ce projet les raycasts ainsi que la vue en FPS.

- https://www.youtube.com/watch?v=v1UGTTeQzbo : quelques éléments de la vidéo 2D Character Health Bars in Unity / 2021 par Distorted Pixel Studios ont été utilisés pour créer un slider prenant le rôle d'une barre de vie en haut à gauche de l'écran.

- https://github.com/amassonie/unitydemo_raycast : toute ressemblance entre les robots d'EscapeTheDrones avec ceux du projet d'Alexandre, Alexis et Clément est justifiée car il s'agit bel et bien du même prefab. Ils avaient le mérite d'être peu lourds en espace en plus de ressembler à des Daleks.

- https://assetstore.unity.com/packages/3d/vehicles/air/simple-drone-190684 : ce modèle a été utilisé pour les drones.