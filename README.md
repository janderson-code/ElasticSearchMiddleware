# ElasticSearchMiddleware para API ASPNET
Este projeto é uma biblioteca de classes desenvolvida em ASP.NET Core 8 , sendo um Middleware para injetar logs da sua API  para o ElasticSearch.

## **Tecnologias Utilizadas**
- **ASP.NET Core 8.0**
- **[Pacote Nuget Nest](https://www.nuget.org/packages/nest)**

### **Clonar o Repositório**
```bash
git clone https://github.com/janderson-code/ElasticSearchMiddleware.git
```

# Api Configuração:
Abaixo as configurações para poder usar esta biblioteca, lembrando que antes deve adicionar a referência desse projeto na sua API aspnetCore 8 


### **Adicionar este projeto como referência na sua API**

### AppSettings da sua API:
![Captura de tela de 2024-10-02 14-23-35](https://github.com/user-attachments/assets/f23d87b0-f4af-4071-bb56-33a8cf9922d9)


### ServiceCollection:
![Captura de tela de 2024-10-02 14-24-00](https://github.com/user-attachments/assets/1269ef1b-1b5c-4b35-a0f6-2e19a8e6d223)

### AplicationBuilder:
![image](https://github.com/user-attachments/assets/37d10f83-9af2-4503-8554-e2153bf73b39)

## Configuration

Rodar o comando Docker compose up no arquivo dockerComposeYml na raiz do projeto para rodar os elasic,kibana e apm.
```bash
  cd ElasticSearchMiddleware
  docker compose up
```

- O elastic rodará na porta 9200
- O Kibana na porta 5601
- O Elastic APM na porta 8200
---
# ATRIBUTOS:

## Atributo pra não gravar requisições de Determinada Controller
Caso queira que uma controller não tenha sua requisições indexadas para o Elastic adicione o atributo abaixo

![image](https://github.com/user-attachments/assets/0c514e27-daa5-4bc0-af9f-5ea8a2e5eee7)


## Atributo para mascarar dados sensíves
Para tipos strings, pode-se mascarar dados do request e response de uma controller. Abaixo o exemplo de uma classe de response.

![Screenshot_20241129_025844](https://github.com/user-attachments/assets/c2f0d2e6-a3f4-41ea-bd31-159d44911cd1)

Decore a propriedade com SensitiveData que recebe como paramêtro um enum do tipo SensitiveLevel onde temos as opções:

<ul>
  <li>CreditCard: Mostrará apenas os 4 primeiros e 4 ultimos dos digitos</li>
  <li>Document:  Mostrará apenas os 3 primeiros e  2 ultimos digitos no caso de cpf e 3 primeiros e 3 ultimos do cnpj</li>  
  <li>InitialDigits: Mostrará apenas os 3 primeiros dígitos</li>
  <li>LastDigits: Mostrará apenas os 3 últimos dígitos</li>
</ul>

### Abaixo como ficará no Kibana : 
![Peek 2024-11-29 03-26](https://github.com/user-attachments/assets/f70546df-c583-4641-8e35-aaa98515d154)





