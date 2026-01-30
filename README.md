# CityWatch: Digital Twin Pothole System
**Combined Project (Unity + React)**

This repository contains the full source code for the "Dead Pixel" submission. It is divided into two main parts:

## 1. The Field App (Unity/AR)
*   **Folder:** `/` (Root Directory, `Assets`, `ProjectSettings`, etc.)
*   **Purpose:** Runs on an Android phone. Uses AR to detect potholes and send reports.
*   **How to Open (For Developers/Judges):**
    1.  Install **Unity Hub** and **Unity 6 (6000.0.x)**.
    2.  Open Unity Hub -> **Add** -> Select this root folder `CIH-Dead_Pixel_9.6_SDG9`.
    3.  Open the project.
*   **How to Run (For Demo):**
    *   Build the APK: `TrafficCity -> Build Android APK`.
    *   Install `CityWatchAR.apk` on an Android phone.

## 2. The Admin Dashboard (Web)
*   **Folder:** `admin-dashboard/`
*   **Purpose:** Runs in a web browser. Receives live data from the Field App and shows analytics.
*   **How to Run:**
    1.  Open Terminal / Command Prompt.
    2.  Navigate to the folder: `cd admin-dashboard`
    3.  Install dependencies (first time only): `npm install`
    4.  Start the server: `npm start`
    5.  Open browser to: `http://localhost:3000`

## Connection Info
*   **Database:** Supabase (Connected via REST API)
*   **Realtime:** Enabled via Supabase subscriptions.
*   **Sync Logic:**
    *   Field App sends POST request -> Supabase DB.
    *   Supabase DB bumps 'Update' -> Admin Dashboard (React) shows instant pop-up.

---
**Team**: Dead Pixel
**Event**: Hackathon 2026
