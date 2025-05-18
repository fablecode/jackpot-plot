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

export interface PagedTickets {
  totalItems: number;
  totalFilteredItems: number;
  totalPages: number;
  tickets: PagedTicket[];
}

export interface PagedTicket {
  ticketId: string;
  ticketName: string;
  lotteryName: string;
  entries: number;
  status: 'active' | 'paused' | 'excluded';
  confidence: 'high' | 'medium' | 'low' | 'none';
}
