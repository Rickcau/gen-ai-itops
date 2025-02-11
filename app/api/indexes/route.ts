const url = `${API_BASE_URL}/indexes/${encodeURIComponent(indexName)}`
console.log('Next.js API Route: Creating index at:', url)

await fetchFromApi(url, {
  method: 'POST'
})

return createApiResponse({ success: true, message: `Successfully created index: ${indexName}` }) 