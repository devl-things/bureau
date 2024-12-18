Here is the **API documentation** for a CRUD Recipes REST API with search functionality. Each endpoint is described with request and response JSON examples.

---

## **1. Create Recipe**

**Endpoint**: `POST /api/recipes`  
**Description**: Create a new recipe.

### Request Body (JSON)
```json
{
  "name": "Francuska salata",
  "ingredients": [
    "Krumpir 500g",
    "Mrkva 300g",
    "Grašak 200g",
    "Kiseli krastavci 100g",
    "Majoneza 200g",
    "Jaja 3 komada"
  ],
  "instructions": "Skuhajte krumpir i mrkvu, dodajte majonezu i promiješajte.",
  "preparationTime": "45 minuta",
  "servings": 6
}
```

### Response (201 Created)
```json
{
  "status": "success",
  "data": {
    "id": 1,
    "name": "Francuska salata",
    "ingredients": [
      "Krumpir 500g",
      "Mrkva 300g",
      "Grašak 200g",
      "Kiseli krastavci 100g",
      "Majoneza 200g",
      "Jaja 3 komada"
    ],
    "instructions": "Skuhajte krumpir i mrkvu, dodajte majonezu i promiješajte.",
    "preparationTime": "45 minuta",
    "servings": 6,
    "createdAt": "2024-06-10T12:00:00Z",
    "updatedAt": "2024-06-10T12:00:00Z"
  }
}
```

---

## **2. Get All Recipes**

**Endpoint**: `GET /api/recipes`  
**Description**: Retrieve a list of all recipes.

### Response (200 OK)
```json
{
  "status": "success",
  "data": [
    {
      "id": 1,
      "name": "Francuska salata",
      "ingredients": [
        "Krumpir 500g",
        "Mrkva 300g",
        "Grašak 200g",
        "Kiseli krastavci 100g",
        "Majoneza 200g",
        "Jaja 3 komada"
      ],
      "preparationTime": "45 minuta",
      "servings": 6,
      "createdAt": "2024-06-10T12:00:00Z",
      "updatedAt": "2024-06-10T12:00:00Z"
    },
    {
      "id": 2,
      "name": "Pileći rižoto",
      "ingredients": [
        "Pileće meso 300g",
        "Riža 200g",
        "Luk 1 komad",
        "Mrkva 100g"
      ],
      "preparationTime": "30 minuta",
      "servings": 4,
      "createdAt": "2024-06-10T12:00:00Z",
      "updatedAt": "2024-06-10T12:00:00Z"

    }
  ]
}
```

---

## **3. Get a Single Recipe**

**Endpoint**: `GET /api/recipes/{id}`  
**Description**: Retrieve a single recipe by ID.

### Example Request
`GET /api/recipes/1`

### Response (200 OK)
```json
{
  "status": "success",
  "data": {
    "id": 1,
    "name": "Francuska salata",
    "ingredients": [
      "Krumpir 500g",
      "Mrkva 300g",
      "Grašak 200g",
      "Kiseli krastavci 100g",
      "Majoneza 200g",
      "Jaja 3 komada"
    ],
    "instructions": "Skuhajte krumpir i mrkvu, dodajte majonezu i promiješajte.",
    "preparationTime": "45 minuta",
    "servings": 6,
    "createdAt": "2024-06-10T12:00:00Z",
    "updatedAt": "2024-06-10T12:00:00Z"
  }
}
```

---

## **4. Update Recipe**

**Endpoint**: `PUT /api/recipes/{id}`  
**Description**: Update an existing recipe.

### Request Body (JSON)
```json
{
  "name": "Francuska salata - osvježena",
  "ingredients": [
    "Krumpir 500g",
    "Mrkva 300g",
    "Grašak 200g",
    "Kiseli krastavci 150g",
    "Majoneza 250g",
    "Jaja 4 komada"
  ],
  "instructions": "Skuhajte krumpir i mrkvu, dodajte više majoneze i promiješajte.",
  "preparationTime": "50 minuta",
  "servings": 8
}
```

### Response (200 OK)
```json
{
  "status": "success",
  "data": {
    "id": 1,
    "name": "Francuska salata - osvježena",
    "ingredients": [
      "Krumpir 500g",
      "Mrkva 300g",
      "Grašak 200g",
      "Kiseli krastavci 150g",
      "Majoneza 250g",
      "Jaja 4 komada"
    ],
    "instructions": "Skuhajte krumpir i mrkvu, dodajte više majoneze i promiješajte.",
    "preparationTime": "50 minuta",
    "servings": 8,
    "createdAt": "2024-06-10T12:00:00Z",
    "updatedAt": "2024-06-10T12:30:00Z"
  }
}
```

---

## **5. Delete Recipe**

**Endpoint**: `DELETE /api/recipes/{id}`  
**Description**: Delete a recipe by ID.

### Example Request
`DELETE /api/recipes/1`

### Response (200 OK)
```json
{
  "status": "success",
  "message": "Recipe deleted successfully."
}
```

---

## **Summary of Endpoints**

| Method | Endpoint               | Description              |
|--------|------------------------|--------------------------|
| POST   | `/api/recipes`         | Create a new recipe      |
| GET    | `/api/recipes`         | Get all recipes          |
| GET    | `/api/recipes/{id}`    | Get a single recipe      |
| PUT    | `/api/recipes/{id}`    | Update a recipe          |
| DELETE | `/api/recipes/{id}`    | Delete a recipe          |


---

## **Updated API Documentation: GET `/api/recipes`**

**Endpoint**: `GET /api/recipes`  
**Description**: Retrieve a list of all recipes with optional filtering parameters.

---

### **Optional Query Parameters**

| Parameter          | Type    | Description                           | Example                       |
|---------------------|---------|---------------------------------------|--------------------------------|
| `name`             | string  | Filter by recipe name (partial match).| `?name=salata`                |
| `preparation_time` | integer | Filter recipes with prep time (in min).| `?preparation_time=45`        |
| `servings`         | integer | Filter recipes by number of servings. | `?servings=6`                 |

---

### **Example 1: Get All Recipes**

**Request**:
```http
GET /api/recipes
```

**Response** (200 OK):
```json
{
  "status": "success",
  "data": [
    {
      "id": 1,
      "name": "Francuska salata",
      "ingredients": [
        "Krumpir 500g",
        "Mrkva 300g",
        "Grašak 200g",
        "Majoneza 200g"
      ],
      "preparation_time": 45,
      "servings": 6
    },
    {
      "id": 2,
      "name": "Pileći rižoto",
      "ingredients": [
        "Pileće meso 300g",
        "Riža 200g",
        "Mrkva 100g"
      ],
      "preparation_time": 30,
      "servings": 4
    }
  ]
}
```

---

### **Example 2: Filter by Name**

**Request**:
```http
GET /api/recipes?name=salata
```

**Response** (200 OK):
```json
{
  "status": "success",
  "data": [
    {
      "id": 1,
      "name": "Francuska salata",
      "ingredients": [
        "Krumpir 500g",
        "Mrkva 300g",
        "Grašak 200g",
        "Majoneza 200g"
      ],
      "preparation_time": 45,
      "servings": 6
    }
  ]
}
```

---

### **Example 3: Filter by Preparation Time**

**Request**:
```http
GET /api/recipes?preparation_time=45
```

**Response** (200 OK):
```json
{
  "status": "success",
  "data": [
    {
      "id": 1,
      "name": "Francuska salata",
      "ingredients": [
        "Krumpir 500g",
        "Mrkva 300g",
        "Grašak 200g",
        "Majoneza 200g"
      ],
      "preparation_time": 45,
      "servings": 6
    }
  ]
}
```

---

### **Example 4: Filter by Multiple Criteria**

**Request**:
```http
GET /api/recipes?name=rižoto&servings=4
```

**Response** (200 OK):
```json
{
  "status": "success",
  "data": [
    {
      "id": 2,
      "name": "Pileći rižoto",
      "ingredients": [
        "Pileće meso 300g",
        "Riža 200g",
        "Mrkva 100g"
      ],
      "preparation_time": 30,
      "servings": 4
    }
  ]
}
```

---

### **Example 5: No Matching Results**

**Request**:
```http
GET /api/recipes?name=pizza
```

**Response** (404 Not Found):
```json
{
  "status": "error",
  "message": "No recipes found matching the criteria."
}
```

---

### **Error Responses**

1. **Invalid Query Parameter**:
   ```json
   {
     "status": "error",
     "message": "Invalid query parameter provided."
   }
   ```

2. **Generic Server Error (500)**:
   ```json
   {
     "status": "error",
     "message": "An unexpected error occurred."
   }
   ```

---

### **Summary of Query Parameters**

| Query Parameter     | Type    | Description                           |
|----------------------|---------|---------------------------------------|
| `name`              | string  | Partial match on recipe name.         |
| `preparation_time`  | integer | Exact match for preparation time (min).|
| `servings`          | integer | Exact match for number of servings.   |

---