# TP 8INF956 - SOA

Il y a 4 projets exécutables et 4 librairies.

## Exécutables : Services et application client
- Client :
    - `ClientConsoleApp` : l'application client console qui demande à l'utilisateur de s'**authentifier** puis affiche une interface simple de shopping si l'authentification réussit. L'utilisateur peut **voir** les produits, les **acheter** ou les **relacher** et afficher sa facture. Cet exécutable utilise les SDK suivants : `UserSDK` pour l'authentification, `StockSDK` pour la partie shopping et `BillSDK` pour la facturation.
- Services:
    - `UserManager`: Chargement des utilisateurs via un fichier JSON. Traite les requêtes d'authentification.

    - `StockManager`: Chargement du stock. Gère les requêtes de visualisation, d'achat, et relachage.

    - `BillManager`: Gère les requête de facturation

Les services et les SDK utilisent la librairie `MessagingSDK` pour envoyer/recevoir des requêtes/réponses via RabbitMQ (par de code RabbitMQ autre part que dans ce SDK).


## Utilisation

Lancement de tous les services :
```
cd UserManager
dotnet run

cd StockManager
dotnet run

cd BillManager
dotnet run
```

Lancement de l'interface cliente :
```
cd ClientConsoleApp
dotnet run
```

Cela devrait vous demander votre username :
```
Username: // taper votre username (ex: smatt7)
```

Une fois connecté (si l'username existe), vous acceder à l'interface de shopping :
```
USER: smatt7: Shaina Matt - smatt7@globo.com

PRODUCTS:
Guava (29.3$)
Carroway Seed (26.5$)
Amaretto (74.8$)
Garlic Powder (33.6$)
Quinoa (22.6$)
Juice - Apple, 341 Ml (7.2$)
Onions Granulated (28.9$)
Capicola - Hot (44.1$)
Wine - Touraine Azay - Le - Rideau (51.6$)
Yogurt - Plain (40.3$)

CARD:
Empty card

B: Buy item     R: Release item    F: Print bill
ACTION (B/R/F):
```

L'utilisateur connecté, la liste des produits recuperée via le service des stock ainsi que le panier de l'utilisateur sont affichés.

Trois actions possibles :
- `B` pour Acheter un nouveau produit
- `R` pour relacher un produit
- `F` pour finaliser la commande (facturation)

Acheter un nouveau produit :
```
ACTION (B/R/F): B
CHOICE (product name): Yogurt - Plain
QUANTITY (number): 10
Not enough quantity in stock !
```

Relacher un produit :
```
ACTION (B/R/F): R
PRODUCT TO RELEASE (product name): Guava
Product released !
```

Facturation :
```
ACTION (B/R/F): F
```
```
CHECKOUT
Facture de smatt7: 8$
Ajout des taxes (20%) : 9.6$
Details:
Name              Qt    Price Alone  Total Price
Guava             2     2$           4$
Yogurt - Plain    2     2$           4$
Bye.
```

## Communication (requêtes / réponses)
Les messages transmis via les `queues` RabbitMQ sont des JSONs pour des soucis d'uniformité et d'extensibilité.

### `UsersManager`
```
// requête
{
    "username": "Romain"
}

// exemple réponse succès
{
    "user": [objet User serialisé]
}

// exemple réponse échec
{
    "error": "Cet utilisateur n'existe pas"
}
```

### `StockManager`
```
// requête
{
    "action": reserve  // actions possibles: ["list", "reserve", "release"]
    "product": "Guava",
    "quantity": 2
}

// exemple réponse succès
{
    "nReserved": 2
}

// exemple réponse échec
{
    "error": "Not enough quantity in stock"
}
```

### `BillManager`
```
// requête
{
    "user": ...,
    "products": ...  
}

// exemple réponse succès
{
    "nReleased": 2
}
```
