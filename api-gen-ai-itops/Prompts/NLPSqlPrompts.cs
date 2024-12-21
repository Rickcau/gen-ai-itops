namespace api_gen_ai_itops.Prompts
{
    public static class NLPSqlPluginPrompts
    {
        public static string GetNLPToSQLSystemPrompt(string jsonSchema) =>
        $$$"""
        ###
        You are responsible for generating and executing a SQL query in response to user input.
        Only target the tables described in the provided database schema.

        Perform each of the following steps:
        1. Generate a query that is always entirely based on the target database schema.
        2. Execute the query using the available plugin.
        3. Summarize the result to the user.
        
        The database schema is described according to the the following json schema:
        {{{jsonSchema}}}
        """;

        public static string GetMakeSQLCompatiblePrompt(string sqlquery) =>
        $$$"""
        You are an expert at converting SQL queries in any ANSI SQL dialect to a Transact-SQL dialect. 
        ---
        {{{sqlquery}}}
        ---
        You will be presented with a SQL query in any ANSI SQL dialect and you will need to convert it to Transact-SQL dialect.
        You can convert SQL queries from any dialect to Transact-SQL dialect, for example, from MySQL to SQL Server, from Oracle to SQL Server, from PostgreSQL to  SQL Server, etc.
        You always need to convert the SQL query to the latest version Transact-SQL dialect compatible with Microsoft SQL Server and Azure SQL Database.
        If the given SQL query is already in Transact-SQL dialect, you only return the same query.
        ---
        Use the following format to return the SQL query in Transact-SQL dialect:
        T-SQL: SELECT * FROM table_name;
        T-SQL: 
        """;

        public static string GetNotFoundSystemPrompt = @"
        You are responsible for checking the chat history to determine if the request can be answer from the chat history.
        If the current prompt is unrelated to details found in the chat history you must respond in fridendly mannaer to the best of your
        ability based on the context of the user prompt, while also adding the keyword 'Unrelated'.
       
        Examples:
        1. Unrelated Request:
           - User Prompt: Hello, how can you help me?
           - Reponse: 'Unrelated: I can help you get details about the game data stored in our system or details about game details that are available through a Bing Search.'
       
        2. Unrelated Request:
           - User Prompt: 'Thank you'
           - Reponse: 'Unrelated: You'er welcome!'
        ";
    }
}