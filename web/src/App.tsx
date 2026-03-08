import React from 'react'
import './App.css'
import ProjectList from './ProjectList'
import UserSwitcher from './UserSwitcher'

function App() {
  return (
    <div id="app-root">
      <header>
        <h1>OpenFGA Example — Projects UI</h1>
        <p>Simple demo UI that calls the API endpoints and shows results.</p>
      </header>

      <div className="layout">
        <div>
          <UserSwitcher />
        </div>
        <div>
          <ProjectList />
        </div>
      </div>
    </div>
  )
}

export default App
