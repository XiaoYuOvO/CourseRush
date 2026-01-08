# CourseRush 📚🚀

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License: LGPL v2](https://img.shields.io/badge/License-LGPL%20v2-blue.svg)](https://www.gnu.org/licenses/old-licenses/lgpl-2.0.html)

**CourseRush** is a high-concurrency course registration assistant for Windows, built with **C# 8.0** and **WPF** on **.NET 8.0 Desktop**. Designed with extensibility in mind, it abstracts core functionalities to support various university course registration systems—currently featuring full support for **Hunan University (HNU)**.

> ⚠️ **Beta Release**: This project is currently in beta. Use at your own risk.

---

## ✨ Features

- **High-Concurrency Course Sniping**: Optimized for rapid, parallel course selection attempts.
- **Multi-School Architecture**: Core logic is abstracted to allow easy integration with different university systems.
- **Automatic Retry**: Failed attempts are automatically retried based on configurable strategies.
- **Course Schedule Viewer**: Visualize your current timetable in a clean UI.
- **Task Group Sequencing**: Execute course selection tasks in user-defined order within task groups.
- **Modular Design**: Clean separation of concerns across authentication, core logic, school-specific implementations, and UI.


---
## 🏫 Supported Universities 
| Name | Status            |
| --------- |-------------------|
| Hunan University | ✅ Fully supported |
---

## 🧩 Project Structure

| Module | Description |
|--------|-------------|
| `CourseRush.Core` | Core abstractions: course models, task definitions, and HTTP request interfaces. |
| `CourseRush.Auth` | Authentication & authorization layer with abstract login chains; includes `CourseRush.Auth.HNU` for Hunan University. |
| `CourseRush.HNU` | Concrete implementation for Hunan University’s latest course registration API. |
| `CourseRush` (UI) | WPF-based user interface using **MVVM**, powered by **HandyControl** and **MahApps.Metro** for responsive, modern UX. |

---

## 🖥️ System Requirements

- **OS**: Windows 10 or later
- **Runtime**: [.NET 8.0 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Architecture**: x64 recommended

---

## 🚀 Getting Started

### Prerequisites

Ensure you have the **.NET 8.0 Desktop Runtime** installed on your system.

### Building from Source

1. Clone the repository:
   ```bash
   git clone https://github.com/your-username/CourseRush.git
   cd CourseRush
   ```

2. Build the solution:
   ```bash
   dotnet build
   ```

3. Run the application:
   ```bash
   dotnet run --project CourseRush/CourseRush.csproj
   ```

> 💡 No precompiled binaries are provided at this time. You must build from source.

---

## 🔧 Usage

1. Launch the application.
2. Log in using your **Hunan University** credentials (currently the only supported institution).
3. Select the course selection sessions
4. Search for courses in course data table using the searching panel
5. Select your wanted courses and right-click to submit them to the task list
6. You can configure the task retry strategy in the task list
7. View your current course schedule in the course table viewer

> Note: There is no initial setup wizard or configuration file yet. All settings are managed through the UI.

---

## 📄 License

This project is licensed under the **GNU Lesser General Public License v2.1 (LGPL-2.1)**.
See [LICENSE](LICENSE) for details.

---

## 🤝 Contributing

While external contributions are not formally documented yet, the modular architecture makes it straightforward to add support for new universities:

1. Implement authentication in `CourseRush.Auth.{University}`
2. Provide course/task logic in a new `CourseRush.{University}` module
3. Register your university in UI module (see `CourseRush.Universities.cs`)

Feel free to open issues for bugs or feature requests!

---

> **Disclaimer**: This tool is intended for educational and personal use only. Users are responsible for complying with their institution’s terms of service and academic policies.