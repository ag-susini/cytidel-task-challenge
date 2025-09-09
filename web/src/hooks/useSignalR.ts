import { useEffect, useRef } from 'react';
import { HubConnection, HubConnectionBuilder, LogLevel, HubConnectionState } from '@microsoft/signalr';
import { useQueryClient } from '@tanstack/react-query';

const SIGNALR_URL = import.meta.env.VITE_API_URL || 'http://localhost:5080';

export const useSignalR = () => {
  const connectionRef = useRef<HubConnection | null>(null);
  const queryClient = useQueryClient();
  const isConnectingRef = useRef(false);

  useEffect(() => {
    // Don't create multiple connections
    if (connectionRef.current && connectionRef.current.state !== HubConnectionState.Disconnected) {
      return;
    }

    const hubUrl = `${SIGNALR_URL}/hubs/tasks`;
    console.log(`[SignalR] Attempting to connect to: ${hubUrl}`);
    
    const connection = new HubConnectionBuilder()
      .withUrl(hubUrl, {
        withCredentials: false
      })
      .withAutomaticReconnect([0, 2000, 10000, 30000])
      .configureLogging(LogLevel.Information)
      .build();

    connectionRef.current = connection;

    // Set up event handlers before connecting
    connection.on('TaskCreated', (data: { taskId: string, title: string }) => {
      console.log('Task created - refreshing data', data);
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      queryClient.invalidateQueries({ queryKey: ['taskStats'] });
    });

    connection.on('TaskUpdated', (data: { taskId: string, title: string }) => {
      console.log('Task updated - refreshing data', data);
      queryClient.invalidateQueries({ queryKey: ['task', data.taskId] });
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      queryClient.invalidateQueries({ queryKey: ['taskStats'] });
    });

    connection.on('TaskDeleted', (data: { taskId: string }) => {
      console.log('Task deleted - refreshing data', data);
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      queryClient.invalidateQueries({ queryKey: ['taskStats'] });
    });

    connection.on('HighPriorityTaskChanged', (data: { taskId: string, title: string, reason: string }) => {
      console.log('High priority task changed - refreshing data', data);
      queryClient.invalidateQueries({ queryKey: ['task', data.taskId] });
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      queryClient.invalidateQueries({ queryKey: ['taskStats'] });
    });

    // Handle reconnection events
    connection.onreconnecting(() => {
      console.log('SignalR reconnecting...');
    });

    connection.onreconnected(() => {
      console.log('SignalR reconnected successfully');
    });

    connection.onclose((error) => {
      console.log('SignalR connection closed', error);
      isConnectingRef.current = false;
    });

    const startConnection = async () => {
      if (isConnectingRef.current || connection.state !== HubConnectionState.Disconnected) {
        return;
      }

      isConnectingRef.current = true;

      try {
        await connection.start();
        console.log('SignalR connected successfully');
      } catch (error) {
        console.warn('SignalR connection failed:', error);
      } finally {
        isConnectingRef.current = false;
      }
    };

    // Start connection with a small delay to avoid race conditions
    const timeoutId = setTimeout(startConnection, 100);

    return () => {
      clearTimeout(timeoutId);
      isConnectingRef.current = false;
      if (connectionRef.current) {
        connectionRef.current.stop().catch(() => {
          // Ignore errors during cleanup
        });
        connectionRef.current = null;
      }
    };
  }, [queryClient]);

  return connectionRef.current;
};