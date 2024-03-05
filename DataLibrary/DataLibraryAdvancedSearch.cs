using AngelHornetLibrary;
using System.Text.RegularExpressions;
using static AngelHornetLibrary.AhLog;

namespace DataLibrary

{
    public static class DataLibraryAdvancedSearch
    {
        public static List<string> ShortHelpText()
        {
            return
            [
                "Simple Search:",
                "   Simple_Search_Text",
                "Advanced Search:",
                "   OPERATOR Search_Text OPERATOR Search_Text ... ",
                "   ",
                "OPERATORS: SEARCH ?? IS == OR || AND &&  NOT !!",
                "Properties: Title, Artist, Album, Genre, Path",
                "Examples:",
                "   Springsteen",
                "   IS Any SEARCH Springst OR Petty OR Seger",
                "   IS Any SEARCH Soundtrack IS Genre AND Country",
                "   IS Any ?? Soundtrack IS Genre && Country",
                "   IS Genre SEARCH Soft Rock OR Heavy Metal",
                "   == Genre ?? Soft Rock || Heavy Metal",
                "   IS Path ?? Lang NOT Slang IS Artist NOT Lange",
                "   == Path ?? Lang !! Slang == Artist !! Lange"
            ];
        }

        // Advanced Search Parsing:
        // (Implied CurrentSet)  OPERATOR Search_Text OPERATOR Search_Text OPERATOR Search_Text ...
        // (Implied CurrentSet)  (Implied SEARCH) Search_Text OPERATOR Search_Text OPERATOR Search_Text ...

        // OPERATORS: (AND|OR|NOT|IS|SEARCH)  ... Must be ALL CAPS to be recognzied as an operator
        // SYMBOLS:    &&  ||  !! ==   ??
        // PROPERTIES: Title, Artist, Album, Genre, Path

        // CurrentSet is always implied.
        // SEARCH is ONLY implied at the beginning of the search string.
        //        IS Genre Rock          -   is NOT a valid, because search is ONLY implied at the beginning of the search string.
        //        IS Genre SEARCH Rock   -   is valid.
        //        IS Genre ?? Rock       -   is valid.

        // Search Syntax Table:
        //| - CurrentSet - | -- Oper - Symb -- | --- Text --- | - Description - 
        //    (Implied)      (Implied Search)    Search Text    Create a new 'Search' discarding the current set   
        //    (Implied)        Search   ??       Rock           Create a new Search for 'Rock' discarding the current set   
        //    (Implied)          IS     ==       Title          Any following command(s) search the 'Title' Property                                  
        //    (Implied)          AND    &&       Heavy Metal    (Union) of the CurrentSet and "Heavy Metal" 
        //    (Implied)          OR     ||       Soft Rock      (Intersection) of the CurrentSet and "Soft Rock"
        //    (Implied)          NOT    !!       Metallica      (Except) of the CurrentSet and "Metallica"

        // Example:
        // "IS Genre ?? Soft Rock OR Heavy Metal == ARTIST NOT Metallica"
        // Will search Genre for "Soft Rock" or "Heavy Metal" and then search Artist for "Metallica" and then remove those from the set.
        // Triple Tuple Trouble!
        public static (List<Song>?, string?, string?) AdvancedSearch(List<Song> _currentSet, string _searchString, string _searchBy = "Any", string _searchAction = "SEARCH")
        {
            Regex _regexSearchOperators = new Regex(@"^\s*(AND|OR|NOT|IS|SEARCH|&&|\|\||!!|==|\?\?)\s(.*)$");
            Regex _regexSearchStrings = new Regex(@"\s+(AND|OR|NOT|IS|SEARCH|&&|\|\||!!|==|\?\?)\s+(\w+)");
            Regex _regexSearchEndsWith = new Regex(@".*\s*(AND|OR|NOT|IS|SEARCH|&&|\|\||!!|==|\?\?)\s*$");
            Dictionary<string, string> _translateCommands = new Dictionary<string, string>
            {
                { "AND", "AND" },
                { "OR", "OR" },
                { "NOT", "NOT" },
                { "IS", "IS" },
                { "SEARCH", "SEARCH" },
                { "&&", "AND" },
                { "||", "OR" },
                { "!!", "NOT" },
                { "==", "IS" },
                { "??", "SEARCH" }
            };


            LogTrace($"Parse:   CurrentSet: {_currentSet.Count},   SearchBy: {_searchBy},   SearchString: [{_searchString}]");
            _searchString = _searchString.Trim();
            var _searchErrorString = _searchString;  // save this in case we error out later
            var _matchOp = _regexSearchOperators.Match(_searchString);
            string _operator1 = "";
            string _operator2 = "";
            string _search = "";

            // Operator
            if (_matchOp.Success)
            {
                _operator1 = _matchOp.Groups[1].Value;
                _searchString = _matchOp.Groups[2].Value;
            }
            else
            {
                _operator1 = _searchAction.ToUpper();
            }

            // SearchString
            var _matchStr = _regexSearchStrings.Match(_searchString);
            var _matchEnd = _regexSearchEndsWith.Match(_searchString);
            if (_matchStr.Success)
            {
                _operator2 = _matchStr.Groups[1].Value;
                _search = _searchString.Substring(0, _matchStr.Groups[1].Index).Trim();
                _searchString = _searchString.Substring(_matchStr.Groups[1].Index).Trim();
            }
            else if (_searchErrorString.Length == 0 || (_searchString.Length > 0 && !_matchEnd.Success))  // _searchErrorString.Length == 0 means we did a blank search, which we want to translate to ALL SONGS.
            {
                LogTrace($"isEmpty[{_searchErrorString.Length}],   Search[{_searchString.Length}]:[{_searchString}],   EndMatch[{_matchEnd.Success}]");
                _search = _searchString;
                _searchString = "";
                if (_searchErrorString.Length == 0)  // cjm - mixed logic, we need to split this out
                {
                    _search = "";
                    _searchBy = "Any";
                    _operator1 = "SEARCH";
                }
            }
            else
            {
                LogWarn($"Syntax Error: {_searchErrorString}");
                var _opError = $"[{_operator1}]";
                var _byError = $"[{_searchBy}]";
                LogDebug($"{_opError,-8} {_byError,-8} [{_search}]  :: [{_operator2}] / [{_searchString}]");
                _searchString = "";
                return (null, null, null);
            }


            _operator1 = _translateCommands[_operator1];
            var _opLog = $"[{_operator1}]";
            var _byLog = $"[{_searchBy}]";
            if (_operator1 == "IS") _byLog = "";
            if (AhLog._LoggingLevel.MinimumLevel < Serilog.Events.LogEventLevel.Debug)
                LogTrace($"{_opLog,-8} {_byLog,-8} [{_search}]  :: [{_operator2}] / [{_searchString}]");
            else
                LogMsg($"{_opLog,-8} {_byLog,-8} [{_search}]");   // <=== Primary MSG for Info and Debug ***

            // Triple Tuple Trouble!
            _searchAction = _operator1;
            (_currentSet, _searchBy) = ExecuteSearchCommand(_currentSet, _operator1, _search, _searchBy);

            if (_searchString.Length > 0)
                (_currentSet, _searchBy, _searchAction) = AdvancedSearch(_currentSet, _searchString, _searchBy, _searchAction);

            LogTrace($"Returning:   " +
                $"CurrentSet: {(_currentSet != null ? _currentSet.Count : "null")},   " +
                $"SearchBy: {(_searchBy != null ? _searchBy : "null")},   " +
                $"SearchAction: {(_searchAction != null ? _searchAction : "null")}");
            return (_currentSet, _searchBy, _searchAction);
        }


        // Double Tuple Trouble!
        public static (List<Song>, string) ExecuteSearchCommand(List<Song> _currentSet, string _operator, string _search, string _searchBy)
        {
            // cj - CONVERT everything to CONCRETE LISTS, ***NOT*** IQueryable
            // IQueryable should be used with caution in Union, Intersect, and Except.  Unless using EmptyOrDefault, and even then, it's still a bit finicky.

            var _by = _searchBy.ToLower();
            _by = _by.Substring(0, 1).ToUpper() + _by.Substring(1);  // Pascal Case
            _search = _search.ToLower();





            // === ====================================================  // cjm
            // vvv vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
            // {Any} {Title} {Artist} {Album} {Band} {Genre}
            // ---
            //var _db = new PlaylistContext();
            //List<Song> _selectionSet = new List<Song>();
            //if (_by == "Any" || _by == "Title") { _selectionSet = _selectionSet.Union(_db.Songs.Where(s => s.Title.ToLower().Contains(_search))).ToList(); }
            //if (_by == "Any" || _by == "Artist") { _selectionSet = _selectionSet.Union(_db.Songs.Where(s => s.Artist.ToLower().Contains(_search))).ToList(); }
            //if (_by == "Any" || _by == "Album") { _selectionSet = _selectionSet.Union(_db.Songs.Where(s => s.Album.ToLower().Contains(_search))).ToList(); }
            //if (_by == "Any" || _by == "Band") { _selectionSet = _selectionSet.Union(_db.Songs.Where(s => s.Band.ToLower().Contains(_search))).ToList(); }
            //if (_by == "Any" || _by == "Genre") { _selectionSet = _selectionSet.Union(_db.Songs.Where(s => s.Genre.ToLower().Contains(_search))).ToList(); }
            //if (_by == "Any" || _by == "Path") { _selectionSet = _selectionSet.Union(_db.Songs.Where(s => s.PathName.ToLower().Contains(_search))).ToList(); }
            //if (!(new string[] { "Any", "Title", "Artist", "Album", "Band", "Genre", "Path" }.Contains(_by))) LogWarn($"Invalid SearchBy: [{_by}]");
            // ---
            // ^^^ ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            // === ====================================================

            // === ====================================================
            // vvv vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
            // {Any} {Title} {Artist} {Album} {Band} {Genre}
            // ---
            var _repository = (ISongRepository)new SongRepository(new PlaylistContext());
            List<Song> _selectionSet = _repository.SearchQuery(_by, _search).Result;            // cjm cjm2
            if (!(new string[] { "Any", "Title", "Artist", "Album", "Band", "Genre", "Path" }.Contains(_by))) LogWarn($"Invalid SearchBy: [{_by}]");
            LogTrace($"SelectionSet: {_selectionSet.Count}");
            // ---
            // ^^^ ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            // === ====================================================







            // {Search} {Or (union)} {And (intersection)} {Not (except)}
            var _action = _operator;
            List<Song> _result = new List<Song>();
            if (_action == "SEARCH") { _result = _selectionSet; }
            else if (_action == "OR") _result = _currentSet.UnionBy(_selectionSet, s => s.Id).ToList();
            // cj - (O.O)!  Why do these not all use the same syntax?!?
            // var intersect = elements.IntersectBy(elements2.Select(e => e.X), x => x.X);
            else if (_action == "AND") { _result = _currentSet.IntersectBy(_selectionSet.Select(s => s.Id), c => c.Id).ToList(); }
            else if (_action == "NOT") { _result = _currentSet.ExceptBy(_selectionSet.Select(s => s.Id), c => c.Id).ToList(); }
            else if (_action == "IS")
            {
                _search = _search.Substring(0, 1).ToUpper() + _search.Substring(1); // Pascal Case
                if (!(new string[] { "Any", "Title", "Artist", "Album", "Band", "Genre", "Path" }.Contains(_search))) LogWarn($"Invalid SearchBy: [{_search}]");
                else _by = _search;
                _result = _currentSet;
            }
            else LogWarn($"Invalid SearchAction: [{_action}]");

            var _songList = _result.ToList();
            LogTrace($"CurrentSet: {_currentSet.Count},   SelectionSet: {_selectionSet.Count},   SearchBy: {_by},   Action: {_action},   Result: {_result.Count}");

            return (_songList, _by);
        }
    }
}
