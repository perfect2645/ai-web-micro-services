import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import type { User } from '../../types/types.ts';

interface LoginProps {
  onLogin: (user: User) => void;
}

const Login = ({ onLogin }: LoginProps) => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!username || !password) return;

    setLoading(true);
    // 模拟登录请求
    setTimeout(() => {
      const user: User = {
        username,
        token: `token_${Date.now()}`,
      };
      onLogin(user);
      setLoading(false);
      navigate('/recognition');
    }, 1000);
  };

  return (
    <div className="container">
      <form className="card login-form" onSubmit={handleSubmit}>
        <h2 style={{ marginBottom: '20px', textAlign: 'center' }}>用户登录</h2>
        <div>
          <label htmlFor="username" style={{ display: 'block', marginBottom: '8px' }}>
            用户名
          </label>
          <input
            type="text"
            id="username"
            className="input"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            placeholder="请输入用户名"
            required
          />
        </div>
        <div>
          <label htmlFor="password" style={{ display: 'block', marginBottom: '8px' }}>
            密码
          </label>
          <input
            type="password"
            id="password"
            className="input"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="请输入密码"
            required
          />
        </div>
        <button type="submit" className="btn" style={{ width: '100%' }} disabled={loading}>
          {loading ? '登录中...' : '登录'}
        </button>
      </form>
    </div>
  );
};

export default Login;