# ElasticSearchMiddleware to aspnet API

## Configuration

Rodar o comando Docker compose up no arquivo dockerComposeYml na raiz do projeto para rodar os elasic,kibana e apm.


## Api Configuration:
Abaixo as configurações para poder usar esta biblioteca, lembrando que antes deve adicionar a referência desse projeto na sua API aspnetCore 8 

### AppSettings:
![Captura de tela de 2024-10-02 14-23-35](https://github.com/user-attachments/assets/f23d87b0-f4af-4071-bb56-33a8cf9922d9)


### ServiceCollection:
![Captura de tela de 2024-10-02 14-24-00](https://github.com/user-attachments/assets/1269ef1b-1b5c-4b35-a0f6-2e19a8e6d223)

### AplicationBuilder:
![image](https://github.com/user-attachments/assets/37d10f83-9af2-4503-8554-e2153bf73b39)


## Atributo pra não gravar requisições de Determinada Controller
Caso queira que uma controller não tenha sua requisições indexadas para o Elastic adicione o atributo abaixo

![image](https://github.com/user-attachments/assets/0c514e27-daa5-4bc0-af9f-5ea8a2e5eee7)



