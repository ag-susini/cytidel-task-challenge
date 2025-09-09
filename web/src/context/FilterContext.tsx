import React, { createContext, useContext, useState, useCallback, useMemo, type ReactNode } from 'react';

interface FilterState {
  status: string;
  priority: string;
  search: string;
  sortBy: string;
  sortOrder: 'asc' | 'desc';
}

interface FilterContextType {
  filters: FilterState;
  updateFilter: <K extends keyof FilterState>(key: K, value: FilterState[K]) => void;
  clearFilters: () => void;
}

const defaultFilters: FilterState = {
  status: 'all',
  priority: 'all',
  search: '',
  sortBy: 'createdAt',
  sortOrder: 'desc',
};

const FilterContext = createContext<FilterContextType | undefined>(undefined);

export const FilterProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [filters, setFilters] = useState<FilterState>(defaultFilters);

  const updateFilter = useCallback(<K extends keyof FilterState>(key: K, value: FilterState[K]) => {
    setFilters(prev => ({ ...prev, [key]: value }));
  }, []);

  const clearFilters = useCallback(() => {
    setFilters(defaultFilters);
  }, []);

  const value = useMemo(() => ({
    filters,
    updateFilter,
    clearFilters
  }), [filters, updateFilter, clearFilters]);

  return (
    <FilterContext.Provider value={value}>
      {children}
    </FilterContext.Provider>
  );
};

export const useFilters = () => {
  const context = useContext(FilterContext);
  if (context === undefined) {
    throw new Error('useFilters must be used within a FilterProvider');
  }
  return context;
};