import React from 'react'
import './App.css'
import { BrowserRouter, Routes, Route } from 'react-router-dom'
import ProjectList from './ProjectList'
import HeaderUserMenu from './HeaderUserMenu'
import EditProject from './EditProject'

function App() {
  return (
    <div id="app-root" style={{ padding: '20px' }}> 
      <header className="app-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div>
          <h1>OpenFGA Example — Projects UI</h1>
          <p>Simple demo UI that calls the API endpoints and shows results.</p>
        </div>
        <div style={{ display: 'flex', alignItems: 'center' }}>
          <HeaderUserMenu />
        </div>
      </header>

      <div className="layout" >
        <main className="content full">
          <BrowserRouter>
            <Routes>
              <Route path="/" element={<ProjectList />} />
              <Route path="/projects/:id/edit" element={<EditProject />} />
            </Routes>
          </BrowserRouter>
        </main>
      </div>
    </div>
  )
}

export default App
