import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'

// Initialize theme from localStorage before React renders
const initializeTheme = () => {
  const savedTheme = localStorage.getItem('theme');
  const isDark = savedTheme ? savedTheme === 'dark' : true;
  
  if (isDark) {
    document.documentElement.classList.add('dark');
  } else {
    document.documentElement.classList.remove('dark');
  }
};

// Apply theme immediately
initializeTheme();

createRoot(document.getElementById('root')!).render(
  <App />
)
