const form = document.getElementById('loginForm');
const responseDiv = document.getElementById('response');

form.addEventListener('submit', async (e) => {
  e.preventDefault();
  
  const username = document.getElementById('username').value;
  const password = document.getElementById('password').value;

  try {
    const res = await fetch('http://localhost:8080/api/Finance/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password })
    });

    const data = await res.json();
    responseDiv.innerText = JSON.stringify(data, null, 2);
  } catch (err) {
    responseDiv.innerText = 'Error: ' + err;
  }
});
