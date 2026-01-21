# Frontend for the Backend Developer Technical Assessment

A small frontend UI is also included in the project to reflect any changes to the DB.
Full CRUD operations can be made using the UI.

## Tech stack used for the frontend:

- **React 18** with TypeScript
- **Vite** - Fast build tool
- **Tailwind CSS** - Styling with dark mode support
- **React Router** - Navigation
- **Axios** - HTTP client
- **React Hook Form** - Form handling
- **Lucide React** - Icons

## Getting Started

### Prerequisites

- Node.js 18+ installed
- Backend API running (default: http://localhost:5000)

### Installation

```bash
cd frontend
npm install
```

### Development

```bash
npm run dev
```

The app will be available at `http://localhost:3000`



## Configuration

Create a `.env` file in the frontend directory:

```env
VITE_API_BASE_URL=http://localhost:5000
```

By default, the development server proxies `/api` requests to `http://localhost:5000`.

## API Endpoints

The frontend connects to the following backend endpoints:

- `GET/POST /api/departments` - Department CRUD
- `GET/POST /api/employees` - Employee CRUD
- `GET/POST /api/projects` - Project CRUD
- `POST/DELETE /api/employees/{id}/projects/{projectId}` - Assignment management
