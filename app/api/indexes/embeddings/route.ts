const url = `${API_BASE_URL}/indexes/embeddings?indexName=${encodeURIComponent(indexName)}`
console.log('Next.js API Route: Generating embeddings at:', url)

await fetchFromApi(url, {
  method: 'POST'
})

return createApiResponse({ success: true, message: `Successfully generated embeddings for index: ${indexName}` }) 