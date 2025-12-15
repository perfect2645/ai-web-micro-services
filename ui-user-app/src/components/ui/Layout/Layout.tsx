import { Link, useNavigate } from 'react-router-dom';
import type { User } from '../../../types/types.ts';

interface LayoutProps {
  user: User | null;
  onLogout: () => void;
  children: React.ReactNode;
}

const Layout = ({ user, onLogout, children }: LayoutProps) => {
  const navigate = useNavigate();

  const handleLogout = () => {
    onLogout();
    navigate('/login');
  };

  if (!user) return <>{children}</>;

  return (
    <div>
      <nav className="nav">
        <Link to="/recognition" className="nav-link">图片识别</Link>
        <Link to="/history" className="nav-link">历史记录</Link>
        <Link to="/settings" className="nav-link">设置</Link>
        <span className="logout-btn" onClick={handleLogout}>退出登录</span>
      </nav>
      <div className="container">{children}</div>
    </div>
  );
};

export default Layout;