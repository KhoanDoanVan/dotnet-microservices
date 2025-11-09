import React, { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { Toaster } from 'react-hot-toast';
// import Login from './pages/Login';
import Dashboard from './pages/Dashboard.tsx';
// import Products from './pages/Products';
// import Orders from './pages/Orders';
// import Customers from './pages/Customers';
// import Inventory from './pages/Inventory';
// import Promotions from './pages/Promotions';
// import Analytics from './pages/Analytics';
import Layout from './components/Layout.tsx';
import { AuthProvider, useAuth } from './context/AuthContext.tsx';

function PrivateRoute({ children }: { children: React.ReactNode }) {
  const { token } = useAuth();
  return token ? <>{children}</> : <Navigate to="/login" />;
}

function App() {
  return (
    <AuthProvider>
      <Router>
        <div className="min-h-screen bg-gray-50">
          <Toaster position="top-right" />
          <dev>Hello AdminUI</dev>
          <Routes>
            {/* <Route path="/login" element={<Login />} /> */}
            <Route
              path="/*"
              element={
                <PrivateRoute>
                  <Layout>
                    <Routes>
                      <Route path="/" element={<Dashboard />} />
                      {/* <Route path="/products" element={<Products />} />
                      <Route path="/orders" element={<Orders />} />
                      <Route path="/customers" element={<Customers />} />
                      <Route path="/inventory" element={<Inventory />} />
                      <Route path="/promotions" element={<Promotions />} />
                      <Route path="/analytics" element={<Analytics />} /> */}
                    </Routes>
                  </Layout>
                </PrivateRoute>
              }
            />
          </Routes>
        </div>
      </Router>
    </AuthProvider>
  );
}

export default App;