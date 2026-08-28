# Trading Journal - Client (Frontend)

This directory contains the frontend application for the Trading Journal platform, built with **Vue 3** and **Vite**. It serves as the user-facing interface for tracking active trades, DCA investments, and overall portfolio performance.

## 🚀 Technologies Used
* **Framework**: Vue 3 (Composition API / `<script setup>`)
* **Build Tool**: Vite
* **Styling**: Tailwind CSS v4 & custom vanilla CSS
* **Routing**: Vue Router
* **HTTP Client**: Axios
* **Icons**: Inline SVG / Heroicons

## 📁 Directory Structure & Categories

### `/src/views`
Contains all the main page components of the application.
* **`auth/`**: Authentication pages (`LoginView.vue`, `RegisterView.vue`).
* **`main/`**: Core application features (`HomeView.vue`, `ActiveTradingView.vue`, `InvestingTrackerView.vue`, `SettingsView.vue`).

### `/src/services`
Houses isolated modules responsible for making HTTP requests to the backend API.
* **`auth.js`**: Handles login, registration, and token management.
* **`dashboard.js`**: Fetches aggregate metrics and timeline activity for the Command Center.
* **`activetrading.js`**: Manages logging active trades and uploading chart images.
* **`settings.js`**: Manages user configurations, such as custom trading strategies.
* **`investing.js`**: Handles DCA campaign logging.

### `/src/router`
Configuration for `vue-router`. Contains route definitions and navigation guards to protect authenticated routes and redirect unauthenticated users securely.

### `/src/utils`
Utility functions and configurations, prominently containing `axios.js` which sets up the interceptors to attach the JWT Bearer token to all outgoing authenticated requests.

### `/src/assets`
Contains global stylesheets (`main.css`), Tailwind directives, and static assets (images, fonts).

## ⚙️ Setup & Installation

1. Navigate to the client directory:
   ```sh
   cd client
   ```
2. Install dependencies:
   ```sh
   npm install
   ```
3. Run the development server:
   ```sh
   npm run dev
   ```
4. Build for production:
   ```sh
   npm run build
   ```
