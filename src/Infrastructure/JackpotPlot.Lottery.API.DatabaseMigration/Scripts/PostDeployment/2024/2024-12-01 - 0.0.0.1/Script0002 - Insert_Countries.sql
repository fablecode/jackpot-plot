-- Africa
DO $$ 
DECLARE
    africa_id INT;
BEGIN
    SELECT id INTO africa_id FROM continents WHERE name = 'Africa';
    INSERT INTO countries (name, country_code, continent_id) VALUES
    ('Algeria', 'DZ', africa_id),
    ('Angola', 'AO', africa_id),
    ('Benin', 'BJ', africa_id),
    ('Botswana', 'BW', africa_id),
    ('Burkina Faso', 'BF', africa_id),
    ('Burundi', 'BI', africa_id),
    ('Cameroon', 'CM', africa_id),
    ('Cape Verde', 'CV', africa_id),
    ('Central African Republic', 'CF', africa_id),
    ('Chad', 'TD', africa_id),
    ('Comoros', 'KM', africa_id),
    ('Congo (Congo-Brazzaville)', 'CG', africa_id),
    ('Djibouti', 'DJ', africa_id),
    ('Egypt', 'EG', africa_id),
    ('Equatorial Guinea', 'GQ', africa_id),
    ('Eritrea', 'ER', africa_id),
    ('Eswatini (fmr. "Swaziland")', 'SZ', africa_id),
    ('Ethiopia', 'ET', africa_id),
    ('Gabon', 'GA', africa_id),
    ('Gambia', 'GM', africa_id),
    ('Ghana', 'GH', africa_id),
    ('Guinea', 'GN', africa_id),
    ('Guinea-Bissau', 'GW', africa_id),
    ('Ivory Coast', 'CI', africa_id),
    ('Kenya', 'KE', africa_id),
    ('Lesotho', 'LS', africa_id),
    ('Liberia', 'LR', africa_id),
    ('Libya', 'LY', africa_id),
    ('Madagascar', 'MG', africa_id),
    ('Malawi', 'MW', africa_id),
    ('Mali', 'ML', africa_id),
    ('Mauritania', 'MR', africa_id),
    ('Mauritius', 'MU', africa_id),
    ('Morocco', 'MA', africa_id),
    ('Mozambique', 'MZ', africa_id),
    ('Namibia', 'NA', africa_id),
    ('Niger', 'NE', africa_id),
    ('Nigeria', 'NG', africa_id),
    ('Rwanda', 'RW', africa_id),
    ('Sao Tome and Principe', 'ST', africa_id),
    ('Senegal', 'SN', africa_id),
    ('Seychelles', 'SC', africa_id),
    ('Sierra Leone', 'SL', africa_id),
    ('Somalia', 'SO', africa_id),
    ('South Africa', 'ZA', africa_id),
    ('South Sudan', 'SS', africa_id),
    ('Sudan', 'SD', africa_id),
    ('Tanzania', 'TZ', africa_id),
    ('Togo', 'TG', africa_id),
    ('Tunisia', 'TN', africa_id),
    ('Uganda', 'UG', africa_id),
    ('Zambia', 'ZM', africa_id),
    ('Zimbabwe', 'ZW', africa_id);
END $$;

-- Antarctica
DO $$ 
DECLARE
    antarctica_id INT;
BEGIN
    SELECT id INTO antarctica_id FROM continents WHERE name = 'Antarctica';
    INSERT INTO countries (name, country_code, continent_id) VALUES
    ('Antarctica', 'AQ', antarctica_id);
END $$;

-- Asia
DO $$ 
DECLARE
    asia_id INT;
BEGIN
    SELECT id INTO asia_id FROM continents WHERE name = 'Asia';
    INSERT INTO countries (name, country_code, continent_id) VALUES
    ('Afghanistan', 'AF', asia_id),
    ('Armenia', 'AM', asia_id),
    ('Azerbaijan', 'AZ', asia_id),
    ('Bahrain', 'BH', asia_id),
    ('Bangladesh', 'BD', asia_id),
    ('Bhutan', 'BT', asia_id),
    ('Brunei', 'BN', asia_id),
    ('Cambodia', 'KH', asia_id),
    ('China', 'CN', asia_id),
    ('Cyprus', 'CY', asia_id),
    ('Georgia', 'GE', asia_id),
    ('India', 'IN', asia_id),
    ('Indonesia', 'ID', asia_id),
    ('Iran', 'IR', asia_id),
    ('Iraq', 'IQ', asia_id),
    ('Israel', 'IL', asia_id),
    ('Japan', 'JP', asia_id),
    ('Jordan', 'JO', asia_id),
    ('Kazakhstan', 'KZ', asia_id),
    ('Kuwait', 'KW', asia_id),
    ('Kyrgyzstan', 'KG', asia_id),
    ('Laos', 'LA', asia_id),
    ('Lebanon', 'LB', asia_id),
    ('Malaysia', 'MY', asia_id),
    ('Maldives', 'MV', asia_id),
    ('Mongolia', 'MN', asia_id),
    ('Myanmar (Burma)', 'MM', asia_id),
    ('Nepal', 'NP', asia_id),
    ('North Korea', 'KP', asia_id),
    ('Oman', 'OM', asia_id),
    ('Pakistan', 'PK', asia_id),
    ('Palestine State', 'PS', asia_id),
    ('Philippines', 'PH', asia_id),
    ('Qatar', 'QA', asia_id),
    ('Saudi Arabia', 'SA', asia_id),
    ('Singapore', 'SG', asia_id),
    ('South Korea', 'KR', asia_id),
    ('Sri Lanka', 'LK', asia_id),
    ('Syria', 'SY', asia_id),
    ('Tajikistan', 'TJ', asia_id),
    ('Thailand', 'TH', asia_id),
    ('Timor-Leste', 'TL', asia_id),
    ('Turkey', 'TR', asia_id),
    ('Turkmenistan', 'TM', asia_id),
    ('United Arab Emirates', 'AE', asia_id),
    ('Uzbekistan', 'UZ', asia_id),
    ('Vietnam', 'VN', asia_id),
    ('Yemen', 'YE', asia_id);
END $$;

-- Europe
DO $$ 
DECLARE
    europe_id INT;
BEGIN
    SELECT id INTO europe_id FROM continents WHERE name = 'Europe';
    INSERT INTO countries (name, country_code, continent_id) VALUES
    ('Albania', 'AL', europe_id),
    ('Andorra', 'AD', europe_id),
    ('Austria', 'AT', europe_id),
    ('Belarus', 'BY', europe_id),
    ('Belgium', 'BE', europe_id),
    ('Bosnia and Herzegovina', 'BA', europe_id),
    ('Bulgaria', 'BG', europe_id),
    ('Croatia', 'HR', europe_id),
    ('Cyprus', 'CY', europe_id),
    ('Czech Republic', 'CZ', europe_id),
    ('Denmark', 'DK', europe_id),
    ('Estonia', 'EE', europe_id),
    ('Finland', 'FI', europe_id),
    ('France', 'FR', europe_id),
    ('Germany', 'DE', europe_id),
    ('Greece', 'GR', europe_id),
    ('Hungary', 'HU', europe_id),
    ('Iceland', 'IS', europe_id),
    ('Ireland', 'IE', europe_id),
    ('Italy', 'IT', europe_id),
    ('Kosovo', 'XK', europe_id),
    ('Latvia', 'LV', europe_id),
    ('Liechtenstein', 'LI', europe_id),
    ('Lithuania', 'LT', europe_id),
    ('Luxembourg', 'LU', europe_id),
    ('Malta', 'MT', europe_id),
    ('Moldova', 'MD', europe_id),
    ('Monaco', 'MC', europe_id),
    ('Montenegro', 'ME', europe_id),
    ('Netherlands', 'NL', europe_id),
    ('North Macedonia', 'MK', europe_id),
    ('Norway', 'NO', europe_id),
    ('Poland', 'PL', europe_id),
    ('Portugal', 'PT', europe_id),
    ('Romania', 'RO', europe_id),
    ('Russia', 'RU', europe_id),
    ('San Marino', 'SM', europe_id),
    ('Serbia', 'RS', europe_id),
    ('Slovakia', 'SK', europe_id),
    ('Slovenia', 'SI', europe_id),
    ('Spain', 'ES', europe_id),
    ('Sweden', 'SE', europe_id),
    ('Switzerland', 'CH', europe_id),
    ('Ukraine', 'UA', europe_id),
    ('United Kingdom', 'GB', europe_id),
    ('Vatican City', 'VA', europe_id);
END $$;

-- North America
DO $$ 
DECLARE
    north_america_id INT;
BEGIN
    SELECT id INTO north_america_id FROM continents WHERE name = 'North America';
    INSERT INTO countries (name, country_code, continent_id) VALUES
    ('Antigua and Barbuda', 'AG', north_america_id),
    ('Bahamas', 'BS', north_america_id),
    ('Barbados', 'BB', north_america_id),
    ('Belize', 'BZ', north_america_id),
    ('Canada', 'CA', north_america_id),
    ('Costa Rica', 'CR', north_america_id),
    ('Cuba', 'CU', north_america_id),
    ('Dominica', 'DM', north_america_id),
    ('Dominican Republic', 'DO', north_america_id),
    ('El Salvador', 'SV', north_america_id),
    ('Grenada', 'GD', north_america_id),
    ('Guatemala', 'GT', north_america_id),
    ('Haiti', 'HT', north_america_id),
    ('Honduras', 'HN', north_america_id),
    ('Jamaica', 'JM', north_america_id),
    ('Mexico', 'MX', north_america_id),
    ('Nicaragua', 'NI', north_america_id),
    ('Panama', 'PA', north_america_id),
    ('Saint Kitts and Nevis', 'KN', north_america_id),
    ('Saint Lucia', 'LC', north_america_id),
    ('Saint Vincent and the Grenadines', 'VC', north_america_id),
    ('Trinidad and Tobago', 'TT', north_america_id),
    ('United States of America', 'US', north_america_id);
END $$;

-- Oceania (Australia)
DO $$ 
DECLARE
    australia_id INT;
BEGIN
    SELECT id INTO australia_id FROM continents WHERE name = 'Australia';
    INSERT INTO countries (name, country_code, continent_id) VALUES
    ('Australia', 'AU', australia_id),
    ('Fiji', 'FJ', australia_id),
    ('Kiribati', 'KI', australia_id),
    ('Marshall Islands', 'MH', australia_id),
    ('Micronesia', 'FM', australia_id),
    ('Nauru', 'NR', australia_id),
    ('New Zealand', 'NZ', australia_id),
    ('Palau', 'PW', australia_id),
    ('Papua New Guinea', 'PG', australia_id),
    ('Samoa', 'WS', australia_id),
    ('Solomon Islands', 'SB', australia_id),
    ('Tonga', 'TO', australia_id),
    ('Tuvalu', 'TV', australia_id),
    ('Vanuatu', 'VU', australia_id);
END $$;

-- South America
DO $$ 
DECLARE
    south_america_id INT;
BEGIN
    SELECT id INTO south_america_id FROM continents WHERE name = 'South America';
    INSERT INTO countries (name, country_code, continent_id) VALUES
    ('Argentina', 'AR', south_america_id),
    ('Bolivia', 'BO', south_america_id),
    ('Brazil', 'BR', south_america_id),
    ('Chile', 'CL', south_america_id),
    ('Colombia', 'CO', south_america_id),
    ('Ecuador', 'EC', south_america_id),
    ('Guyana', 'GY', south_america_id),
    ('Paraguay', 'PY', south_america_id),
    ('Peru', 'PE', south_america_id),
    ('Suriname', 'SR', south_america_id),
    ('Uruguay', 'UY', south_america_id),
    ('Venezuela', 'VE', south_america_id);
END $$;

