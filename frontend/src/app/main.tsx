import { bootstrap } from './bootstrap';
import '@/shared/styles/global.css';

const root = document.getElementById('root');
if (!root) throw new Error('Root element is missing');
void bootstrap(root);
