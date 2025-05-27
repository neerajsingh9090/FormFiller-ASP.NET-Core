----------Key Features------
Bulk URL Processing: Accepts a text file containing multiple URLs and automates form filling across all specified websites.

Dynamic Extra Fields: Users can define custom key-value pairs as extra form data, allowing flexibility for different types of forms across websites.

Automated Form Detection: Utilizes smart heuristics to detect input fields such as name, email, phone, message, company, and subject, filling them based on provided data.

Fallback Navigation: If a form is not detected on the homepage, the application intelligently navigates to common pages like “Contact Us” or “About Us” to find and fill forms.

Real-Time Progress Reporting: Live streaming of automation progress updates and statuses back to the user interface using Server-Sent Events (SSE).

CSV Reporting: Automatically generates downloadable reports in CSV format summarizing the success, failure, or skipping of each processed URL.

Error Handling: Gracefully manages navigation and form filling exceptions, ensuring uninterrupted processing of remaining URLs.

Extensible and Maintainable: Modular architecture separates Selenium automation logic from web UI, making it easy to extend and maintain.

-----------Technologies Used---------
ASP.NET Core MVC: For building the scalable and responsive web application framework.

Selenium WebDriver: Automates browser actions for interaction with web forms.

ChromeDriver: Controls the Google Chrome browser to execute automation tasks.

Server-Sent Events (SSE): Enables real-time communication from the server to the client for live status updates.

Bootstrap 5: Provides a clean, responsive, and user-friendly front-end interface.

C# and .NET 7: Core language and runtime powering the back-end services.

--------Use Cases--------------
Marketing teams needing to submit contact or lead generation forms en masse.

Quality assurance professionals automating form validation and testing on multiple websites.

Data entry automation to reduce manual work and improve accuracy.

Digital agencies managing outreach across many client websites efficiently.

------------How It Works-----------
Input: The user uploads a .txt file containing a list of URLs and enters additional form data fields (key-value pairs) via the UI.

Automation: The backend service uses Selenium to open each URL in a Chrome browser instance, detect form fields, and fill them using the provided data.

Progress: Live status updates are pushed to the client’s browser, showing success, failures, or skipped forms.

Report: After processing, a downloadable CSV report summarizes the result of each URL’s form submission.

This project demonstrates advanced automation capabilities combined with real-time user interaction and reporting, making repetitive web form filling tasks seamless and efficient.

