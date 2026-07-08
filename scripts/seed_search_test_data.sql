-- Seed ~10,000 tasks for full-text search performance testing (US-024).
--
-- Usage (against the local database):
--   docker exec -i flowboard-postgres psql -U flowboard -d flowboard < scripts/seed_search_test_data.sql
--
-- It creates a dedicated seed user, organisation, membership, and project (fixed UUIDs, idempotent),
-- then inserts 10,000 tasks with varied titles and descriptions. The tasks.search_vector generated
-- column and its GIN index (idx_tasks_search) are populated automatically.
--
-- Measuring search performance (target: < 300 ms for the ranked task query on 10k rows):
--   EXPLAIN (ANALYZE, BUFFERS)
--   SELECT id, title,
--          ts_rank(search_vector, websearch_to_tsquery('english', 'payment gateway')) AS rank
--   FROM tasks
--   WHERE organisation_id = '00000000-0000-0000-0000-0000000000aa'
--     AND deleted_at IS NULL
--     AND search_vector @@ websearch_to_tsquery('english', 'payment gateway')
--   ORDER BY rank DESC
--   LIMIT 20;
-- Look for a Bitmap Index Scan on idx_tasks_search and an "Execution Time" line. The endpoint itself
-- can be timed with: curl -o /dev/null -s -w '%{time_total}\n' "<baseUrl>/api/v1/organisations/<org>/search?q=payment+gateway"

-- created_at and joined_at have no database default (the application supplies them), so a raw
-- seed must set them explicitly. updated_at does have a default and is omitted.
INSERT INTO users (id, email, password_hash, display_name, is_email_verified, created_at)
VALUES ('00000000-0000-0000-0000-0000000000a1', 'seed@example.com', 'x', 'Seed User', true, now())
ON CONFLICT (id) DO NOTHING;

INSERT INTO organisations (id, name, slug, owner_id, created_at)
VALUES ('00000000-0000-0000-0000-0000000000aa', 'Seed Org', 'seed-org', '00000000-0000-0000-0000-0000000000a1', now())
ON CONFLICT (id) DO NOTHING;

INSERT INTO memberships (id, user_id, organisation_id, role, joined_at)
VALUES ('00000000-0000-0000-0000-0000000000ab', '00000000-0000-0000-0000-0000000000a1', '00000000-0000-0000-0000-0000000000aa', 'owner', now())
ON CONFLICT (id) DO NOTHING;

INSERT INTO projects (id, organisation_id, name, status, created_by_id, created_at)
VALUES ('00000000-0000-0000-0000-0000000000ac', '00000000-0000-0000-0000-0000000000aa', 'Seed Project', 'active', '00000000-0000-0000-0000-0000000000a1', now())
ON CONFLICT (id) DO NOTHING;

INSERT INTO tasks (id, project_id, organisation_id, title, description, status, priority, created_by_id, created_at)
SELECT
    gen_random_uuid(),
    '00000000-0000-0000-0000-0000000000ac',
    '00000000-0000-0000-0000-0000000000aa',
    (ARRAY['Fix','Improve','Design','Refactor','Investigate','Document','Optimise','Build'])[1 + floor(random() * 8)]
        || ' the '
        || (ARRAY['login','payment gateway','search index','dashboard','notification','export','onboarding','billing'])[1 + floor(random() * 8)]
        || ' '
        || (ARRAY['flow','page','service','pipeline','report','widget','module','job'])[1 + floor(random() * 8)],
    'Detailed notes about the '
        || (ARRAY['authentication','performance','accessibility','localisation','migration','caching','security','reporting'])[1 + floor(random() * 8)]
        || ' work for the '
        || (ARRAY['marketing','internal','customer','admin','mobile','partner'])[1 + floor(random() * 6)]
        || ' team',
    (ARRAY['todo','in_progress','blocked','done'])[1 + floor(random() * 4)],
    (ARRAY['low','medium','high','critical'])[1 + floor(random() * 4)],
    '00000000-0000-0000-0000-0000000000a1',
    now()
FROM generate_series(1, 10000);

ANALYZE tasks;
