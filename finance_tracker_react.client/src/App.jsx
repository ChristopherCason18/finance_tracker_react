import { useState, useRef } from 'react';
import './App.css';
import FileUploader from './upload';
import { AllCommunityModule, ModuleRegistry } from 'ag-charts-community';
import { AgCharts } from 'ag-charts-react';

ModuleRegistry.registerModules([AllCommunityModule]);

const ChartDefault = ({ transactions }) => {
    const totalsByDate = {};
    transactions.forEach(t => {
        const date = t.date;

        if (!totalsByDate[date]) {
            totalsByDate[date] = 0;
        }

        totalsByDate[date] += Number(t.amount);
    });
    const chartData = Object.entries(totalsByDate)
        .map(([date, amount]) => ({
            date,
            amount
        }));
    const chartOptions = {
        data: chartData,
        series: [
            {
                type: 'bar',
                xKey: 'date',
                yKey: 'amount'
            }
        ]
    };
    return (
        <div style={{ height: 400 }}>
            <AgCharts options={chartOptions} />
        </div>
    );
};

const ChartType = ({ transactions }) => {
    const totalsByCode = {};
    transactions.forEach(t => {
        const code = t.code;

        if (!totalsByCode[code]) {
            totalsByCode[code] = 0;
        }

        totalsByCode[code] += Number(t.amount);
    });
    const chartData = Object.entries(totalsByCode)
        .map(([date, amount]) => ({
            date,
            amount
        }));
    const chartOptions = {
        data: chartData,
        series: [
            {
                type: 'bar',
                xKey: 'date',
                yKey: 'amount'
            }
        ]
    };
    return (
        <div style={{ height: 400 }}>
            <AgCharts options={chartOptions} />
        </div>
    );
};

function App() {
    const [transactions, setTransactions] = useState([]);
    const [error, setError] = useState(null);

    const [showChart, setShowChart] = useState(false);

    const uploaderRef = useRef(null);

    function graphData() {
        setShowChart(true);
    }

    async function populateTransactions() {
        try {
            const response = await fetch('/transactions/transactions', {
                credentials: 'include',
            });

            if (!response.ok) {
                setError(response.statusText);
                return;
            }

            const contentType = response.headers.get('content-type') || '';

            if (contentType.includes('application/json')) {
                const data = await response.json();
                setTransactions(Array.isArray(data) ? data : []);
                return;
            }

            const text = await response.text();
            const lines = text.trim().split(/\r?\n/).filter(Boolean);

            if (!lines.length) {
                setTransactions([]);
                return;
            }

            const header = lines[0]
                .split(',')
                .map(h => h.trim().replace(/^"|"$/g, '').toLowerCase());

            const records = lines.slice(1).map((line, idx) => {
                const cols = line.split(',');
                const obj = {};
                header.forEach((h, i) => (obj[h] = cols[i] ?? ''));

                return {
                    id: obj['id'] || idx + 1,
                    date: obj['date'] || '',
                    type: obj['type'] || '',
                    details: obj['details'] || '',
                    particulars: obj['particulars'] || '',
                    code: obj['code'] || '',
                    reference: obj['reference'] || '',
                    amount: Number((obj['amount'] || '0').replace(/[^0-9.-]/g, '')) || 0,
                    balance: Number((obj['balance'] || '0').replace(/[^0-9.-]/g, '')) || 0,
                };
            });

            setTransactions(records);
        } catch (err) {
            setError(err.message);
        }
    }

    if (error) {
        return (
            <div>
                <h1>Transactions</h1>
                <p style={{ color: 'red' }}>Error: {error}</p>
            </div>
        );
    }

    return (
        <div>
            <h1>Transactions</h1>

            <FileUploader ref={uploaderRef} />

            <button onClick={populateTransactions}>
                Load Transactions From Uploaded File
            </button>

            <button onClick={graphData}>Graph Data</button>

            {showChart && (
                <ChartDefault transactions={transactions} />
            )}

            <button onClick={graphData}>Graph Data by Transaction Location</button>

            {showChart && (
                <ChartType transactions={transactions} />
            )}

            <table className="table">
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Date</th>
                        <th>Type</th>
                        <th>Details</th>
                        <th>Particulars</th>
                        <th>Code</th>
                        <th>Reference</th>
                        <th>Amount</th>
                        <th>Balance</th>

                    </tr>
                </thead>

                <tbody>
                    {(Array.isArray(transactions) ? transactions : []).map(t => (
                        <tr key={t.id}>
                            <td>{t.id}</td>
                            <td>{t.date}</td>
                            <td>{t.type}</td>
                            <td>{t.details}</td>
                            <td>{t.particulars}</td>
                            <td>{t.code}</td>
                            <td>{t.reference}</td>
                            <td>{t.amount}</td>
                            <td>{t.balance}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
}

export default App;