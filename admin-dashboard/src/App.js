import React, { useEffect, useState, useRef } from 'react';
import './App.css';
import { supabase } from './supabaseClient';
import maplibregl from 'maplibre-gl';
import 'maplibre-gl/dist/maplibre-gl.css';
import {
  PieChart, Pie, Cell, LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer
} from 'recharts';

function App() {
  const [reports, setReports] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selectedLocation, setSelectedLocation] = useState(null);
  const [lastSync, setLastSync] = useState(null);

  // Map References
  const mapContainer = useRef(null);
  const mapCallback = useRef(null);
  const markersRef = useRef({});

  // Initialize Map
  useEffect(() => {
    if (mapCallback.current) return; // Initialize only once

    const map = new maplibregl.Map({
      container: mapContainer.current,
      style: 'https://tiles.openfreemap.org/styles/liberty',
      center: [79.0882, 21.1458],
      zoom: 15,
      pitch: 50,
      bearing: -10
    });

    map.addControl(new maplibregl.NavigationControl(), 'top-right');
    mapCallback.current = map;

    return () => {
      // Cleanup map on unmount
      if (mapCallback.current) {
        mapCallback.current.remove();
        mapCallback.current = null;
      }
    };
  }, []);

  // Sync Markers with Map
  useEffect(() => {
    if (!mapCallback.current) return;
    const map = mapCallback.current;

    // Clear old markers that are gone
    Object.keys(markersRef.current).forEach((id) => {
      if (!reports.find(r => r.id === parseInt(id))) {
        markersRef.current[id].remove();
        delete markersRef.current[id];
      }
    });

    // Add/Update markers
    reports.forEach(report => {
      if (!markersRef.current[report.id]) {
        // Create DOM element for marker
        const el = document.createElement('div');
        el.className = `map-marker ${report.status}`;
        el.style.fontSize = '24px';
        el.style.cursor = 'pointer';
        el.style.filter = 'drop-shadow(0 2px 4px rgba(0,0,0,0.5))';
        // el.innerText = report.status === 'fixed' || report.status === 'repaired' ? '🟢' : report.status === 'in-progress' ? '🟡' : '🔴';
        el.innerHTML = report.status === 'fixed' || report.status === 'repaired' ? '🟢' : report.status === 'in-progress' ? '🟡' : '🔴';

        el.addEventListener('click', (e) => {
          e.stopPropagation();
          setSelectedLocation(report);
        });

        // Add to map
        const marker = new maplibregl.Marker({ element: el, anchor: 'bottom' })
          .setLngLat([report.lng, report.lat])
          .addTo(map);

        markersRef.current[report.id] = marker;
      } else {
        // Update position if needed (usually reports don't move, but good practice)
        markersRef.current[report.id].setLngLat([report.lng, report.lat]);

        // Update icon/class if status changed
        const el = markersRef.current[report.id].getElement();
        el.className = `map-marker ${report.status}`;
        el.innerHTML = report.status === 'fixed' || report.status === 'repaired' ? '🟢' : report.status === 'in-progress' ? '🟡' : '🔴';
      }
    });

  }, [reports]);

  // Fly to selected location
  useEffect(() => {
    if (selectedLocation && mapCallback.current) {
      mapCallback.current.flyTo({
        center: [selectedLocation.lng, selectedLocation.lat],
        zoom: 17,
        pitch: 60,
        essential: true
      });
    }
  }, [selectedLocation]);


  useEffect(() => {
    fetchInitialData();
    setLastSync(new Date().toLocaleTimeString());

    const channel = supabase
      .channel('realtime-reports')
      .on('postgres_changes', { event: 'INSERT', schema: 'public', table: 'reports' }, (payload) => {
        setReports((prev) => [payload.new, ...prev]);
        setLastSync(new Date().toLocaleTimeString());
      })
      .on('postgres_changes', { event: 'UPDATE', schema: 'public', table: 'reports' }, (payload) => {
        setReports((prev) => prev.map((item) => (item.id === payload.new.id ? payload.new : item)));
        setLastSync(new Date().toLocaleTimeString());
      })
      .on('postgres_changes', { event: 'DELETE', schema: 'public', table: 'reports' }, (payload) => {
        setReports((prev) => prev.filter((item) => item.id !== payload.old.id));
        setLastSync(new Date().toLocaleTimeString());
      })
      .subscribe();

    return () => {
      supabase.removeChannel(channel);
    };
  }, []);

  const fetchInitialData = async () => {
    setLoading(true);
    const { data, error } = await supabase
      .from('reports')
      .select('*')
      .order('id', { ascending: false });

    if (error) {
      console.error('API Error:', error);
    } else {
      setReports(data || []);
      if (data && data.length > 0) setSelectedLocation({ lat: data[0].lat, lng: data[0].lng });
    }
    setLoading(false);
  };

  const updateStatus = async (id, newStatus) => {
    await supabase.from('reports').update({ status: newStatus }).eq('id', id);
  };

  const deleteReport = async (id) => {
    if (window.confirm('Delete this report permanently?')) {
      await supabase.from('reports').delete().eq('id', id);
    }
  };

  // --- Features ---
  const downloadCSV = () => {
    const headers = ['ID,Type,Status,Latitude,Longitude,Created At'];
    const rows = reports.map(r =>
      `${r.id},${r.type},${r.status},${r.lat},${r.lng},${r.created_at}`
    );
    const csvContent = "data:text/csv;charset=utf-8," + [headers, ...rows].join("\n");
    const encodedUri = encodeURI(csvContent);
    const link = document.createElement("a");
    link.setAttribute("href", encodedUri);
    link.setAttribute("download", `citywatch_reports_${Date.now()}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  const simulateIncomingData = () => {
    const mockReport = {
      id: Math.floor(Math.random() * 10000),
      type: 'pothole',
      status: 'pending',
      lat: 21.1458 + (Math.random() * 0.01),
      lng: 79.0882 + (Math.random() * 0.01),
      created_at: new Date().toISOString()
    };
    // Optimistic update for demo
    setReports(prev => [mockReport, ...prev]);
    alert('Simulated Report Added! Check the dashboard.');
  };

  // --- Analytics Data ---
  const getPieData = () => {
    const pending = reports.filter(r => r.status === 'pending').length;
    const fixed = reports.filter(r => r.status === 'fixed' || r.status === 'repaired').length;
    const progress = reports.filter(r => r.status === 'in-progress').length;
    return [
      { name: 'Pending', value: pending, color: '#ef4444' },
      { name: 'In Progress', value: progress, color: '#f59e0b' },
      { name: 'Fixed', value: fixed, color: '#10b981' },
    ].filter(d => d.value > 0);
  };

  const getLineData = () => {
    const counts = {};
    reports.forEach(r => {
      const date = new Date(r.created_at).toLocaleDateString();
      counts[date] = (counts[date] || 0) + 1;
    });
    return Object.keys(counts).map(date => ({ date, count: counts[date] })).reverse().slice(0, 7);
  };

  const totalReports = reports.length;
  const fixedRate = totalReports > 0 ? ((reports.filter(r => r.status === 'fixed' || r.status === 'repaired').length / totalReports) * 100).toFixed(0) : 0;

  return (
    <div className="app-container">
      <aside className="sidebar">
        <h2 className="brand">City<span className="scent">Watch</span> AI</h2>
        <nav>
          <a href="#dashboard" className="active">📊 Dashboard</a>
          <a href="#map">🗺️ Live Map</a>
          <a href="#reports">📝 Reports</a>
          <a href="#settings">⚙️ Settings</a>
        </nav>
      </aside>

      <main className="main-content">
        <header className="top-bar">
          <div className="title-wrapper">
            <h1>Dashboard <span className="live-indicator" title="Live System"></span></h1>
            {lastSync && <div className="last-sync">Last Sync: {lastSync}</div>}
          </div>
          <div className="btn-group">
            <button className="btn btn-secondary" onClick={simulateIncomingData}>⚡ Simulate</button>
            <button className="btn btn-secondary" onClick={downloadCSV}>📥 Export CSV</button>
            <button className="btn btn-primary" onClick={fetchInitialData}>🔄 Refresh</button>
          </div>
        </header>

        {/* Stats */}
        <section className="stats-grid">
          <div className="card">
            <h3>Total Reports</h3>
            <p className="big-number">{totalReports}</p>
          </div>
          <div className="card">
            <h3>Attention Needed</h3>
            <p className="big-number" style={{ color: '#ef4444' }}>
              {reports.filter(r => r.status === 'pending').length}
            </p>
          </div>
          <div className="card">
            <h3>Resolution Rate</h3>
            <p className="big-number" style={{ color: '#10b981' }}>{fixedRate}%</p>
          </div>
        </section>

        {/* Charts */}
        {totalReports > 0 && (
          <section className="analytics-row">
            <div className="card chart-card">
              <h3>Status Overview</h3>
              <ResponsiveContainer width="100%" height={250}>
                <PieChart>
                  <Pie data={getPieData()} innerRadius={60} outerRadius={80} paddingAngle={5} dataKey="value">
                    {getPieData().map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={entry.color} />
                    ))}
                  </Pie>
                  <Tooltip contentStyle={{ backgroundColor: '#1e293b', border: 'none', color: '#fff' }} itemStyle={{ color: '#fff' }} />
                </PieChart>
              </ResponsiveContainer>
            </div>
            <div className="card chart-card">
              <h3>Activity Trend</h3>
              <ResponsiveContainer width="100%" height={250}>
                <LineChart data={getLineData()}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#334155" />
                  <XAxis dataKey="date" stroke="#94a3b8" />
                  <YAxis allowDecimals={false} stroke="#94a3b8" />
                  <Tooltip contentStyle={{ backgroundColor: '#1e293b', border: 'none', color: '#fff' }} />
                  <Line type="monotone" dataKey="count" stroke="#3b82f6" strokeWidth={3} dot={{ r: 4 }} />
                </LineChart>
              </ResponsiveContainer>
            </div>
          </section>
        )}

        {/* Data & Map */}
        <section className="content-split">
          <div className="reports-table-container card">
            <h3>Recent Reports</h3>
            {reports.length === 0 && !loading ? (
              <div className="empty-state">
                <span className="empty-icon">🎉</span>
                <p>No issues reported yet. Good job!</p>
              </div>
            ) : (
              <table className="reports-table">
                <thead>
                  <tr>
                    <th>Type</th>
                    <th>Status</th>
                    <th>GPS Coords</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {reports.map((report) => (
                    <tr key={report.id} onClick={() => setSelectedLocation(report)} className="clickable-row">
                      <td data-label="Type">{report.type}</td>
                      <td data-label="Status">
                        <select
                          className={`status-select ${report.status}`}
                          value={report.status}
                          onChange={(e) => updateStatus(report.id, e.target.value)}
                          onClick={(e) => e.stopPropagation()}
                        >
                          <option value="pending">Pending</option>
                          <option value="in-progress">In Progress</option>
                          <option value="fixed">Fixed</option>
                        </select>
                      </td>
                      <td data-label="GPS" style={{ fontFamily: 'monospace', fontSize: '0.85rem' }}>
                        {report.lat.toFixed(5)},{report.lng.toFixed(5)}
                      </td>
                      <td data-label="Actions" className="actions-cell">
                        <a
                          href={`https://www.google.com/maps?q=${report.lat},${report.lng}`}
                          target="_blank"
                          rel="noreferrer"
                          className="action-btn map-btn"
                          onClick={(e) => e.stopPropagation()}
                          title="Open in Maps"
                        >
                          📍
                        </a>
                        <button
                          className="action-btn delete-btn"
                          onClick={(e) => { e.stopPropagation(); deleteReport(report.id); }}
                          title="Delete"
                        >
                          🗑️
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>

          <div className="mini-map-container card">
            <h3>Live 3D City Map (Free)</h3>
            <div style={{ flex: 1, minHeight: '350px', position: 'relative', borderRadius: '8px', overflow: 'hidden' }}>
              <div ref={mapContainer} style={{ width: '100%', height: '100%' }} />
            </div>
            <p className="map-hint" style={{ fontSize: '0.8rem', color: '#64748b', marginTop: '8px' }}>
              Powered by <b>OpenFreeMap</b> (No Credit Card Required).
            </p>
          </div>
        </section>
      </main>
    </div>
  );
}

export default App;
