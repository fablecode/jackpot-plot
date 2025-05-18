-- Script0010 - Add_UserFilter_To_GetPaginatedTicketOverview.sql

CREATE OR REPLACE FUNCTION get_paginated_ticket_overview_json(    
    page_number INT,
    page_size INT,
	user_id UUID DEFAULT NULL,
    search_term TEXT DEFAULT NULL,
    sort_column TEXT DEFAULT 'ticket_id',
    sort_direction TEXT DEFAULT 'asc'
)
RETURNS JSONB AS $$
DECLARE
    total_filtered_items INT;
    total_items INT;
    total_pg INT;
    filtered_sql TEXT;
    sql TEXT;
    row RECORD;
    tickets JSONB := '[]'::jsonb;
    safe_sort_column TEXT;
BEGIN
    -- Enforce valid sort columns
    safe_sort_column := CASE sort_column
        WHEN 'ticket_id' THEN 'ticket_id'
        WHEN 'ticket_name' THEN 'ticket_name'
        WHEN 'lottery_name' THEN 'lottery_name'
        WHEN 'status' THEN 'status'
        WHEN 'entries' THEN 'entries'
        WHEN 'confidence' THEN 'confidence'
        ELSE 'ticket_id'
    END;

    -- Total items (unfiltered)
    IF user_id IS NOT NULL THEN
        EXECUTE 'SELECT COUNT(*) FROM ticket_overview WHERE user_id = $1'
        INTO total_items
        USING user_id;
    ELSE
        SELECT COUNT(*) INTO total_items FROM ticket_overview;
    END IF;

    -- Build filtered query (user + search)
    filtered_sql := format(
        'FROM ticket_overview WHERE (%s) AND %s',
        '$1 IS NULL OR user_id = $1',
        CASE
            WHEN search_term IS NOT NULL THEN 'ticket_name ILIKE ''%%'' || $2 || ''%%'''
            ELSE 'TRUE'
        END
    );

    -- Count filtered
    EXECUTE format('SELECT COUNT(*) %s', filtered_sql)
    INTO total_filtered_items
    USING user_id, search_term;

    total_pg := CEIL(total_filtered_items::DECIMAL / page_size)::INT;

    -- Paginated result query
    sql := format(
        'SELECT * %s ORDER BY %s %s LIMIT %s OFFSET (%s - 1) * %s',
        filtered_sql,
        safe_sort_column,
        CASE lower(sort_direction)
            WHEN 'desc' THEN 'DESC'
            ELSE 'ASC'
        END,
        page_size,
        page_number,
        page_size
    );

    -- Execute paginated query and build JSON array
    FOR row IN EXECUTE sql USING user_id, search_term LOOP
        tickets := tickets || to_jsonb(row);
    END LOOP;

    -- Return JSON result
    RETURN jsonb_build_object(
        'total_items', total_items,
        'total_filtered_items', total_filtered_items,
        'total_pages', total_pg,
        'tickets', tickets
    );
END;
$$ LANGUAGE plpgsql;