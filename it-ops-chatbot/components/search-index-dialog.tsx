import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Switch } from "@/components/ui/switch"
import { useState } from "react"
import { Loader2 } from "lucide-react"

interface SearchParams {
  query: string
  k: number
  top: number
  filter: string | null
  textOnly: boolean
  hybrid: boolean
  semantic: boolean
  minRerankerScore: number
}

interface SearchIndexDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  indexName: string
  isSearching: boolean
  onSearch: (indexName: string, params: SearchParams) => Promise<void>
  results?: any[]
}

export function SearchIndexDialog({
  open,
  onOpenChange,
  indexName,
  isSearching,
  onSearch,
  results = []
}: SearchIndexDialogProps) {
  const [searchParams, setSearchParams] = useState<SearchParams>({
    query: "",
    k: 3,
    top: 10,
    filter: null,
    textOnly: false,
    hybrid: true,
    semantic: false,
    minRerankerScore: 2
  })

  const handleSearch = () => {
    if (!searchParams.query.trim()) return
    onSearch(indexName, searchParams)
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-4xl max-h-[90vh] flex flex-col">
        <DialogHeader>
          <DialogTitle>Search Index: {indexName}</DialogTitle>
        </DialogHeader>

        <div className="grid gap-4 py-4">
          {/* Query Input */}
          <div className="grid gap-2">
            <Label htmlFor="query">Search Query</Label>
            <Input
              id="query"
              placeholder="Enter your search query..."
              value={searchParams.query}
              onChange={(e) => setSearchParams({ ...searchParams, query: e.target.value })}
              onKeyDown={(e) => {
                if (e.key === 'Enter' && !isSearching) {
                  handleSearch()
                }
              }}
            />
          </div>

          {/* Numeric Parameters */}
          <div className="grid grid-cols-2 gap-4">
            <div className="grid gap-2">
              <Label htmlFor="k">K Value</Label>
              <Input
                id="k"
                type="number"
                min={1}
                value={searchParams.k}
                onChange={(e) => setSearchParams({ ...searchParams, k: Number(e.target.value) })}
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="top">Top Results</Label>
              <Input
                id="top"
                type="number"
                min={1}
                value={searchParams.top}
                onChange={(e) => setSearchParams({ ...searchParams, top: Number(e.target.value) })}
              />
            </div>
          </div>

          <div className="grid gap-2">
            <Label htmlFor="minRerankerScore">Minimum Reranker Score</Label>
            <Input
              id="minRerankerScore"
              type="number"
              min={0}
              step={0.1}
              value={searchParams.minRerankerScore}
              onChange={(e) => setSearchParams({ ...searchParams, minRerankerScore: Number(e.target.value) })}
            />
          </div>

          {/* Toggle Options */}
          <div className="grid grid-cols-2 gap-4">
            <div className="flex items-center justify-between">
              <Label htmlFor="textOnly">Text Only</Label>
              <Switch
                id="textOnly"
                checked={searchParams.textOnly}
                onCheckedChange={(checked) => setSearchParams({ ...searchParams, textOnly: checked })}
              />
            </div>
            <div className="flex items-center justify-between">
              <Label htmlFor="hybrid">Hybrid Search</Label>
              <Switch
                id="hybrid"
                checked={searchParams.hybrid}
                onCheckedChange={(checked) => setSearchParams({ ...searchParams, hybrid: checked })}
              />
            </div>
            <div className="flex items-center justify-between">
              <Label htmlFor="semantic">Semantic Search</Label>
              <Switch
                id="semantic"
                checked={searchParams.semantic}
                onCheckedChange={(checked) => setSearchParams({ ...searchParams, semantic: checked })}
              />
            </div>
          </div>
        </div>

        <div className="flex justify-end gap-2">
          <Button
            variant="outline"
            onClick={() => onOpenChange(false)}
            disabled={isSearching}
          >
            Cancel
          </Button>
          <Button
            onClick={handleSearch}
            disabled={isSearching || !searchParams.query.trim()}
          >
            {isSearching ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Searching...
              </>
            ) : (
              'Search'
            )}
          </Button>
        </div>

        {/* Results Section */}
        <div className="border-t mt-4 pt-4 flex-1 overflow-y-auto">
          <div className="font-medium mb-2">Search Results {results.length > 0 && `(${results.length})`}</div>
          {isSearching ? (
            <div className="flex items-center justify-center p-8">
              <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
            </div>
          ) : results.length > 0 ? (
            <div className="space-y-4">
              {results.map((result, index) => (
                <div 
                  key={index}
                  className="rounded-lg border p-4 hover:bg-muted/50"
                >
                  <div className="flex justify-between items-start mb-2">
                    <div className="font-medium">
                      {result.name || `Result ${index + 1}`}
                    </div>
                    {result.score && (
                      <div className="text-sm text-muted-foreground">
                        Score: {result.score.toFixed(4)}
                      </div>
                    )}
                  </div>
                  <pre className="whitespace-pre-wrap text-sm bg-muted p-2 rounded">
                    {JSON.stringify(result, null, 2)}
                  </pre>
                </div>
              ))}
            </div>
          ) : searchParams.query.trim() ? (
            <div className="text-center text-muted-foreground p-8">
              No results found for your search.
            </div>
          ) : (
            <div className="text-center text-muted-foreground p-8">
              Enter a search query and click Search to see results.
            </div>
          )}
        </div>
      </DialogContent>
    </Dialog>
  )
} 