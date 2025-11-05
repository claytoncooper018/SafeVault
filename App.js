import React, { useState } from 'react';
import api from './api';

export default function App(){
  const [token, setToken] = useState('');
  const [username, setUsername] = useState('admin');
  const [password, setPassword] = useState('AdminPass123');
  const [secrets, setSecrets] = useState([]);
  const [title, setTitle] = useState('');
  const [data, setData] = useState('');

  const login = async (e) => {
    e.preventDefault();
    const res = await api.login({username, password});
    if(res && res.success){ setToken(res.token); alert('Logged in'); }
    else alert('Login failed');
  };

  const load = async () => {
    const res = await api.getSecrets(token);
    if(res && res.success) setSecrets(res.data);
    else alert('Not authorized or error');
  };

  const create = async (e) => {
    e.preventDefault();
    const res = await api.createSecret(token, {title, data});
    if(res && res.success){ alert('Created'); setTitle(''); setData(''); load(); }
    else alert('Error: ' + JSON.stringify(res));
  };

  return (
    <div style={{padding:20}}>
      <h1>SafeVault</h1>
      <form onSubmit={login}>
        <input value={username} onChange={e=>setUsername(e.target.value)} />
        <input type="password" value={password} onChange={e=>setPassword(e.target.value)} />
        <button type="submit">Login</button>
      </form>
      <button onClick={load} style={{marginTop:10}}>Load Secrets</button>
      <h3>Create Secret (Admin)</h3>
      <form onSubmit={create}>
        <input placeholder="Title" value={title} onChange={e=>setTitle(e.target.value)} />
        <input placeholder="Data" value={data} onChange={e=>setData(e.target.value)} />
        <button type="submit">Create</button>
      </form>
      <ul>{secrets.map(s=> <li key={s.id}><b>{s.title}</b>: {s.createdAt}</li>)}</ul>
    </div>
  );
}
