DO $$
DECLARE
    eurojackpot_lottery_id INT;
    germany_country_id INT;
    spain_country_id INT;
    italy_country_id INT;
    denmark_country_id INT;
    netherlands_country_id INT;
    finland_country_id INT;
    iceland_country_id INT;
    sweden_country_id INT;
    croatia_country_id INT;
    norway_country_id INT;
    poland_country_id INT;
    latvia_country_id INT;
    lithuania_country_id INT;
    estonia_country_id INT;
    czech_republic_country_id INT;
    slovakia_country_id INT;
    hungary_country_id INT;
    slovenia_country_id INT;
BEGIN
    -- Retrieve EuroJackpot lottery ID
    SELECT id INTO eurojackpot_lottery_id
    FROM lotteries
    WHERE name = 'EuroJackpot';

    -- Retrieve country IDs
    SELECT id INTO germany_country_id FROM countries WHERE name = 'Germany';
    SELECT id INTO spain_country_id FROM countries WHERE name = 'Spain';
    SELECT id INTO italy_country_id FROM countries WHERE name = 'Italy';
    SELECT id INTO denmark_country_id FROM countries WHERE name = 'Denmark';
    SELECT id INTO netherlands_country_id FROM countries WHERE name = 'Netherlands';
    SELECT id INTO finland_country_id FROM countries WHERE name = 'Finland';
    SELECT id INTO iceland_country_id FROM countries WHERE name = 'Iceland';
    SELECT id INTO sweden_country_id FROM countries WHERE name = 'Sweden';
    SELECT id INTO croatia_country_id FROM countries WHERE name = 'Croatia';
    SELECT id INTO norway_country_id FROM countries WHERE name = 'Norway';
    SELECT id INTO poland_country_id FROM countries WHERE name = 'Poland';
    SELECT id INTO latvia_country_id FROM countries WHERE name = 'Latvia';
    SELECT id INTO lithuania_country_id FROM countries WHERE name = 'Lithuania';
    SELECT id INTO estonia_country_id FROM countries WHERE name = 'Estonia';
    SELECT id INTO czech_republic_country_id FROM countries WHERE name = 'Czech Republic';
    SELECT id INTO slovakia_country_id FROM countries WHERE name = 'Slovakia';
    SELECT id INTO hungary_country_id FROM countries WHERE name = 'Hungary';
    SELECT id INTO slovenia_country_id FROM countries WHERE name = 'Slovenia';

    -- Insert associations into lotteries_countries
    INSERT INTO lotteries_countries (lottery_id, country_id, created_at, updated_at)
    VALUES
        (eurojackpot_lottery_id, germany_country_id, NOW(), NOW()),
        (eurojackpot_lottery_id, spain_country_id, NOW(), NOW()),
        (eurojackpot_lottery_id, italy_country_id, NOW(), NOW()),
        (eurojackpot_lottery_id, denmark_country_id, NOW(), NOW()),
        (eurojackpot_lottery_id, netherlands_country_id, NOW(), NOW()),
        (eurojackpot_lottery_id, finland_country_id, NOW(), NOW()),
        (eurojackpot_lottery_id, iceland_country_id, NOW(), NOW()),
        (eurojackpot_lottery_id, sweden_country_id, NOW(), NOW()),
        (eurojackpot_lottery_id, croatia_country_id, NOW(), NOW()),
        (eurojackpot_lottery_id, norway_country_id, NOW(), NOW()),
        (eurojackpot_lottery_id, poland_country_id, NOW(), NOW()),
        (eurojackpot_lottery_id, latvia_country_id, NOW(), NOW()),
        (eurojackpot_lottery_id, lithuania_country_id, NOW(), NOW()),
        (eurojackpot_lottery_id, estonia_country_id, NOW(), NOW()),
        (eurojackpot_lottery_id, czech_republic_country_id, NOW(), NOW()),
        (eurojackpot_lottery_id, slovakia_country_id, NOW(), NOW()),
        (eurojackpot_lottery_id, hungary_country_id, NOW(), NOW()),
        (eurojackpot_lottery_id, slovenia_country_id, NOW(), NOW());
END $$;
