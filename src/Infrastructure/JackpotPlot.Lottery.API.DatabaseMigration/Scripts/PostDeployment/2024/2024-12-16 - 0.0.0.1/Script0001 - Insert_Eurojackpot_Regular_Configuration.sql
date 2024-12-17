INSERT INTO lottery_configuration (
    lottery_id, 
    draw_type, 
    main_numbers_count, 
    main_numbers_range, 
    bonus_numbers_count, 
    bonus_numbers_range, 
    start_date, 
    created_at, 
    updated_at
)
VALUES (
    (SELECT id FROM lotteries WHERE name = 'EuroJackpot'),  -- Dynamically fetch lottery_id
    'regular',                                                     -- Draw type
    5,                                                             -- Main numbers count
    50,                                                            -- Main numbers range
    2,                                                             -- Bonus numbers count
    12,                                                            -- Bonus numbers range
    '2024-03-23 00:00:00',                                         -- Start date
    NOW(),                                                         -- Created at
    NOW()                                                          -- Updated at
);
