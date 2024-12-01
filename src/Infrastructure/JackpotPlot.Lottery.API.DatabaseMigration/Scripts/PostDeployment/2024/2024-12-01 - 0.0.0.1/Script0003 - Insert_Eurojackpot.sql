INSERT INTO lotteries (name, description, draw_frequency, status, created_at, updated_at)
    VALUES
        (
            'EuroJackpot',
            'EuroJackpot is a transnational European lottery with a 5/50 + 2/10 format. Draws occur weekly, offering large jackpots.',
            'Weekly',
            'active',
            NOW(),
            NOW()
        )