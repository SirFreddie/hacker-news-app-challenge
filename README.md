# 🚀 Hacker News App

This project is a full-stack web application that fetches the **latest stories from Hacker News** using an **ASP.NET Core API** and displays them on an **Angular frontend**. The backend caches the data efficiently to **optimize API calls** while ensuring users see **updated content**.

---

## 📌 Features

### 🔹 **Frontend (Angular)**

- Displays a **list of the newest stories**.
- Each list item includes a **title** and a **clickable link**.
- **Search functionality** to filter stories.
- **Pagination** for better readability.
- Uses **Angular Material** for UI components.
- **Unit tests** for the core functionalities.

### 🔹 **Backend (ASP.NET Core)**

- **RESTful API** to fetch and serve Hacker News stories.
- **Efficient caching strategy** (caches both story IDs & actual stories separately).
- **Updates cache dynamically** when new stories appear.
- Uses **dependency injection** for modular and testable code.
- **Unit tests** to validate functionality.

---

## 🏗️ Tech Stack

### 🔹 **Frontend**

- Angular 19
- Angular Material
- RxJS
- TypeScript

### 🔹 **Backend**

- ASP.NET Core 9
- C#
- HttpClient for external API calls
- IMemoryCache for caching
- xUnit for unit testing

---

## 🚀 Getting Started

### **1️⃣ Backend Setup** (ASP.NET Core 9)

```bash
# Navigate to the backend directory
cd backend/HackerNewsApi

# Restore dependencies
dotnet restore

# Run the API
dotnet run
```

- The backend should now be running on **http://localhost:5149/api**
- The api also contains swagger documentation at **http://localhost:5149/swagger**

---

### **2️⃣ Frontend Setup** (Angular 19)

```bash
# Navigate to the frontend directory
cd frontend/hacker-news-front

# Install dependencies
npm install

# Start the Angular development server
ng serve
```

The frontend should now be running on **http://localhost:4200/**.

---

## 📖 API Endpoints

| Method | Endpoint                                               | Description                  |
| ------ | ------------------------------------------------------ | ---------------------------- |
| GET    | `/api/news?page={page}&pageSize={size}&search={query}` | Fetch paginated news stories |

Example Request:

```
GET http://localhost:5149/api/news?page=1&pageSize=10&search=Angular
```

---

## 🛠️ Running Tests

### **Backend Tests (xUnit)**

```bash
cd backend/HackerNewsApi.Tests

dotnet test
```

### **Frontend Tests (Karma & Jasmine)**

```bash
cd frontend/hacker-news-front

ng test
```

## 🎯 Key Design Decisions

- **Efficient Caching**: Instead of fetching all stories at once, the API caches **story IDs separately** from the actual story data, ensuring **minimal API calls** to Hacker News.
- **Lazy Loading**: The API **only fetches missing stories when needed**, reducing unnecessary network requests.
- **Parallel Processing**: Multiple stories are fetched **in parallel** for better performance.
- **Pagination from Cache**: Users can change **page sizes dynamically** without breaking pagination logic.

---

## 💡 Possible Improvements

- **Background Task for Auto-Refreshing**: Instead of refreshing the cache on user requests, we could run a **scheduled job** to refresh stories in the background.
- **Rate Limiting Handling**: Implement a **retry mechanism** or exponential backoff in case the Hacker News API **rate limits** us.
- **Infinite Scrolling**: Instead of traditional pagination, allow users to **dynamically load more stories** as they scroll.

## ⏳ Total Estimated Development Time

| **Backend Total** | **Frontend Total** | **Overall Total**               |
| ----------------- | ------------------ | ------------------------------- |
| 30 hours          | 10 hours           | **40 hours** (~4 full workdays) |

---

## 👨‍💻 Author

- **Rodrigo Luque**

---

## 📜 License

This project is licensed under the **MIT License**.
