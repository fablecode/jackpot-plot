export interface Ticket {
  id: string;
  name: string;
  isPublic: boolean;
  playCount: number;
  lottery: string;
  status: 'Active' | 'Paused' | 'Excluded';
  nextDraw: Date | null;
  entries: number;
  lastResult: {
    tier: number;
    description: string;
  } | null;
  confidence: 'High' | 'Medium' | 'Low' | 'None';
}
