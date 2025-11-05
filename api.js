const API_BASE = process.env.REACT_APP_API_URL || 'http://localhost:5000';

async function request(path, options = {}){
  try {
    const res = await fetch(API_BASE + path, options);
    const json = await res.json();
    return json;
  } catch(e){
    console.error('API error', e);
    return null;
  }
}

export default {
  login: (creds) => request('/api/auth/login', {
    method: 'POST', headers: {'Content-Type': 'application/json'}, body: JSON.stringify(creds)
  }),
  getSecrets: (token) => request('/api/secrets', {
    headers: token ? {'Authorization': 'Bearer ' + token} : {}
  }),
  createSecret: (token, payload) => request('/api/secrets', {
    method: 'POST',
    headers: {'Content-Type': 'application/json', 'Authorization': 'Bearer ' + token},
    body: JSON.stringify(payload)
  })
};
