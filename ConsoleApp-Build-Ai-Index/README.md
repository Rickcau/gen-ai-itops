# ConsoleApp-Build-Index
I have created a utility that allows you to **create**, **load**, **delete** and **search** the customer index.

## Description
Azure OpenAI Search Helper

### Usage:
 ConsoleApp-Build-Ai-Index **[options]**

#### Options:
   
   ```
      **--create**           Create the search index
      **--load**             Generate and load documents into the index
      **--search <search>** Perform a search with the specified query
      **--delete**          Delete the search index
      **--version**          Show version information
      **-?, -h, --help**     Show help and usage information
   ```

#### Examples:
  
   ~~~
      ConsoleApp-Build-Ai-Index **--create**
      ConsoleApp-Build-Ai-Index **--load**
      ConsoleApp-Build-Ai-Index **--create --load**
      ConsoleApp-Build-Ai-Index **--search** "List VMs"
      ConsoleApp-Build-Ai-Index **--delete**
   ~~~

## Next Steps
Next, we will create a simple **ConsoleApp-Chat-Bot** that has a simple Chat Loop that allows you to start Runbooks, check on their status etc.  The next example **ConsoleApp-Chat-Bot** will combine all the concepts we have learned in the previous examples, this will be the basis for a ChatProvider API that can be used by any Client.



