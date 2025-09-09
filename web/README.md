# Tasker Web - React Frontend Application

## 📋 Description

The Tasker Web application is a modern React 19 frontend built with TypeScript and Vite. It provides a responsive, real-time task management interface with advanced features including lazy loading, visual statistics, and WebSocket-based live updates.

### Frontend Highlights
- **React 19 with TypeScript**: Latest React features with type safety
- **Vite**: Lightning-fast build tool and development server
- **TailwindCSS**: Utility-first CSS framework for rapid UI development
- **TanStack Query**: Powerful data synchronization for server state
- **SignalR Client**: Real-time updates via WebSocket connection
- **Radix UI**: Accessible, unstyled UI components
- **React Hook Form + Zod**: Type-safe form handling and validation

## 🛠️ Prerequisites

- **Node.js 20+** - [Download](https://nodejs.org/)
- **Package Manager**: npm (comes with Node.js), yarn, or pnpm
- **IDE**: VS Code with ESLint and Prettier extensions recommended

## 🚀 Setup Instructions

### 1. Install Dependencies

```bash
cd web
npm install
# or
yarn install
# or
pnpm install
```

### 2. Configure Environment

Create a `.env.local` file for local development (optional):

```env
VITE_API_URL=http://localhost:5080
```

If not set, the app defaults to `http://localhost:5080` for the API.

### 3. Run Development Server

```bash
npm run dev
# or
yarn dev
# or
pnpm dev
```

The application will be available at http://localhost:3000

Features available in development:
- Hot Module Replacement (HMR)
- React Query DevTools
- Source maps for debugging
- TypeScript type checking

### 4. Build for Production

```bash
npm run build
# or
yarn build
# or
pnpm build
```

This creates an optimized production build in the `dist` folder.

## 🎨 Features

### Task Management
- **Task List View**: Paginated list with infinite scroll
- **Task Details**: Full task information with edit capabilities
- **Create/Edit Forms**: Validated forms with real-time feedback
- **Delete Confirmation**: Modal confirmation for destructive actions
- **Advanced Filtering**: Filter by status, priority, and search terms
- **Sorting**: Sort tasks by various fields

### Visual Elements
- **Statistics Dashboard**: Interactive charts using Recharts
  - Task distribution by status
  - Priority breakdown
  - Completion trends
- **Progress Indicators**: Visual task completion progress
- **Status Badges**: Color-coded status and priority indicators
- **Loading Skeletons**: Smooth loading states for better UX

### Real-time Features
- **Live Updates**: Automatic UI updates when tasks change
- **SignalR Integration**: WebSocket connection for real-time events
  - Task created notifications
  - Task updated alerts
  - Task deleted notifications
  - High-priority task changes
- **Optimistic Updates**: Instant UI feedback with rollback on error

### Authentication
- **Login/Register**: Secure authentication forms
- **Protected Routes**: Automatic redirection for unauthenticated users
- **Token Management**: Automatic token refresh and storage
- **Logout**: Clear session and redirect to login

### User Experience
- **Lazy Loading**: Infinite scroll for large task lists
- **Debounced Search**: Efficient search with 500ms debounce
- **Responsive Design**: Mobile-first approach with Tailwind
- **Dark Mode**: Theme switching support (if configured)
- **Toast Notifications**: Success/error feedback using Sonner
- **Form Validation**: Real-time validation with helpful error messages

## 🏗️ Project Structure

```
web/
├── src/
│   ├── api/                   # API client and service layers
│   │   ├── auth.ts            # Authentication API calls
│   │   ├── tasks.ts           # Task CRUD operations
│   │   └── client.ts          # Axios configuration
│   │
│   ├── components/            # Reusable React components
│   │   ├── ui/                # Base UI components (Radix + Tailwind)
│   │   ├── Layout.tsx         # App layout wrapper
│   │   ├── ProtectedRoute.tsx # Route authentication guard
│   │   ├── TaskStatsChart.tsx # Statistics visualization
│   │   └── TaskStatsChartSkeleton.tsx
│   │
│   ├── context/               # React Context providers
│   │   └── AuthContext.tsx    # Authentication state management
│   │
│   ├── hooks/                 # Custom React hooks
│   │   ├── useTasks.ts        # Task data fetching hooks
│   │   ├── useSignalR.ts      # SignalR connection management
│   │   ├── useTheme.ts        # Theme management
│   │   └── use-toast.ts       # Toast notifications
│   │
│   ├── pages/                 # Page components (routes)
│   │   ├── TaskList.tsx       # Main task list view
│   │   ├── TaskDetail.tsx     # Individual task view/edit
│   │   ├── CreateTask.tsx     # New task creation
│   │   ├── Login.tsx          # Authentication page
│   │   └── Register.tsx       # User registration
│   │
│   ├── types/                 # TypeScript type definitions
│   │   ├── Task.ts            # Task entity types
│   │   ├── TaskStatus.ts      # Status enum
│   │   ├── TaskPriority.ts    # Priority enum
│   │   ├── AuthResult.ts      # Auth response types
│   │   └── ...                # Other type definitions
│   │
│   ├── lib/                   # Utility functions
│   │   └── utils.ts           # Helper functions
│   │
│   ├── App.tsx                # Main application component
│   └── main.tsx               # Application entry point
│
├── public/                    # Static assets
├── package.json              # Dependencies and scripts
├── tsconfig.json             # TypeScript configuration
├── vite.config.ts            # Vite configuration
├── tailwind.config.js        # Tailwind CSS configuration
└── eslint.config.js          # ESLint configuration
```

## 🧪 Testing

### Run Tests
```bash
npm test
# or
yarn test
# or
pnpm test
```

### Linting
```bash
npm run lint
# or
yarn lint
# or
pnpm lint
```

## 📝 Development Notes

### Component Architecture
- **Radix UI**: Provides unstyled, accessible components
- **Tailwind CSS**: Handles all styling through utility classes
- **shadcn/ui pattern**: Components in `components/ui` folder
- **CVA (Class Variance Authority)**: For component variants

### State Management
- **TanStack Query**: Server state management
  - Automatic caching
  - Background refetching
  - Optimistic updates
- **React Context**: Authentication state
- **Local State**: Component-specific state with useState

### Data Fetching Patterns
- **Infinite Queries**: For paginated task lists
- **Standard Queries**: For single resources
- **Mutations**: For create/update/delete operations
- **Real-time Updates**: SignalR invalidates queries on server events

### Form Handling
- **React Hook Form**: Form state management
- **Zod**: Schema validation
- **Error Display**: Inline validation messages

## 🔧 Configuration

### Vite Configuration
The app uses Vite for development and building:
- Fast HMR
- TypeScript support
- Path aliases (`@/` for `src/`)
- Environment variable handling

### TailwindCSS Configuration
- Custom color scheme
- Responsive breakpoints
- Animation utilities
- Dark mode support (class-based)

### TypeScript Configuration
- Strict mode enabled
- Path mapping for clean imports
- React JSX support

## 🐛 Troubleshooting

### API Connection Issues
- Verify API is running on http://localhost:5080
- Check CORS settings in API
- Ensure `.env.local` has correct API URL

### SignalR Connection Issues
- Check browser console for WebSocket errors
- Verify `/hubs/tasks` endpoint is accessible
- Ensure CORS allows WebSocket connections

### Build Issues
- Clear node_modules and reinstall: `rm -rf node_modules && npm install`
- Clear Vite cache: `rm -rf node_modules/.vite`
- Check for TypeScript errors: `npm run build`

### Authentication Issues
- Check token storage in localStorage
- Verify JWT configuration matches API
- Clear browser storage and re-login

## 📦 Docker Deployment

Build the production image:

```bash
cd web
docker build -t tasker-web .
docker run -p 3000:3000 tasker-web
```

Or use Docker Compose from the deploy folder for the complete stack.

## 🔗 Related Documentation

- [Main Project README](../README.md)
- [API Documentation](../api/README.md)
- [Deployment Guide](../deploy/README.md)

---

Built with React 19, TypeScript, Vite, and TailwindCSS