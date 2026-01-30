
import { createClient } from '@supabase/supabase-js'

const supabaseUrl = 'https://gozytubnvbnofbraophc.supabase.co'
const supabaseAnonKey = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imdvenl0dWJudmJub2ZicmFvcGhjIiwicm9sZSI6ImFub24iLCJpYXQiOjE3Njk3NjIxNjYsImV4cCI6MjA4NTMzODE2Nn0.kWMFN6M3gr4OKZsQI47e0jPCebPoN-W67-Vi0AMgbCM'

export const supabase = createClient(supabaseUrl, supabaseAnonKey)
