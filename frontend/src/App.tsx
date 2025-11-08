import { useEffect, useState } from 'react'
import './App.css'

interface HealthCheck {
    status: string;
    timestamp: string;
}

function App() {
    const [health, setHealth] = useState<HealthCheck | null>(null);

    useEffect(() => {
        fetch('/api/health')
            .then(r => r.json())
            .then(data => setHealth(data))
            .catch(err => console.error('Failed to fetch health:', err));
    }, []);

    return (
        <>
            <h1>TwoPly - Pair Programming Manager</h1>
            <div className="card">
                <h2>Backend Status</h2>
                {health ? (
                    <div>
                        <p>✅ Status: {health.status}</p>
                        <p>🕐 Timestamp: {new Date(health.timestamp).toLocaleString()}</p>
                    </div>
                ) : (
                    <p>Loading...</p>
                )}
            </div>
        </>
    )
}

export default App