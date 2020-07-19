# BizTalk KAFKA Adapter
BizTalk Kafka adapter provides integration with Kafka broker to send and receive messages from kafka topics. 

# Installation

### BizTalk Supported version
- BizTalk Server 2016

### Instructions
- Download msi from releases\version\ folder
- Download install scripts from install-scripts folder
- From a powershell prompt, run following command:
PS ```install-scripts\InstallRM.ps1 -msiLocation <path of msi> -msiName KafkaAdapter -environment <Environment Name Dev,QA,PROD> -lastServer $true/false```

*Above command uses [BTDF](https://marketplace.visualstudio.com/items?itemName=DeployFxForBizTalkTeam.DeploymentFrameworkforBizTalkToolsforVS2015) MSI deployment feature and can also be used to automate the deployment on multi server environment using TFS. For more detailed instructions on automation, follow the [link](https://vikas15bhardwaj.wordpress.com/2016/09/09/visual-studio-tfs-build-and-release-management-for-biztalk-part-i-introduction/)*

- Restart BizTalk Console and now you should see KAFKA Adapter in the list of adapters. Associate your host and following below instructions for setting up Receive and Send ports

# Configure Send Port
Coming soon

# Configure Receive Location
Coming soon

