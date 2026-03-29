import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { ErrorListPage } from './pages/ErrorListPage';
import { ErrorDetailPage } from './pages/ErrorDetailPage';

export default function App() {
  return (
    <BrowserRouter>
      <main style={{ maxWidth: 1200, margin: '0 auto', fontFamily: 'Arial, sans-serif', padding: 16 }}>
        <Routes>
          <Route path="/" element={<ErrorListPage />} />
          <Route path="/errors/:id" element={<ErrorDetailPage />} />
        </Routes>
      </main>
    </BrowserRouter>
  );
}
