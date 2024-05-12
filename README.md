# WebSearchIndexing
Web Search Indexing is a Blazor application designed to simplify website indexing tasks.

## Description
Web Search Indexing is a user-friendly web application that facilitates the process of indexing your website for google search engine. This application provides an intuitive interface to manage and configure indexing settings.

## Getting Started
### Option 1: Cloning the Repository
Clone the repository to your local machine:
```
git clone https://github.com/QuEng/WebSearchIndexing.git
```

Modify the following configuration parameters in the `docker-compose.yml` file:
* Replace `{YOUR_POSTGRES_PASSWORD}` with your secure for the PostgreSQL database.
* Replace `{YOUR_ACCESS_KEY}` with your secure access key.

Build and run the containers using Docker Compose:
```
docker-compose build
docker-compose up -d
```

Once successfully launched, the application will be accessible at `http://localhost:10005`. (You can change port in `docker-compose.yml`)

### Option 2: Using Docker Image from Docker Hub
Pull the Docker image from Docker Hub:
```
docker pull qu6eng/web-search-indexing
```

Modify the following configuration parameters in the `docker-compose.yml` file:
* Replace `{YOUR_POSTGRES_PASSWORD}` with your secure for the PostgreSQL database.
* Replace `{YOUR_ACCESS_KEY}` with your secure access key.

Build and run the containers using Docker Compose:
```
docker-compose up -d
```

After a successful start, access the application at `http://localhost:10005`. (You can change port in `docker-compose.yml`)


## Usage
1. Log in to the application using your credentials(Access key).
2. Navigate to `Service accounts` page and add your service accounts, which you can create in the Google Cloud console.
3. Navigate to `Urls/All urls` page and add URLs that you need to index. (No more than 10,000 at a time, if uploaded from a file)
4. Navigate to `Settings` page and set a limit for requests per day.
5. Click `Enable service`.
6. Service started!

## Configuring User Secrets for Local Development
For local development, you will need to configure user secrets to provide database connection strings and application access keys.

### Option 1:
Use Visual Studio interface

### Option 2:
1. Install the .NET Core CLI (Command Line Interface):
If you haven't already, install the .NET Core CLI by following the instructions for your operating system [here](https://dotnet.microsoft.com/en-us/download).
2. Set User Secrets for the Project:
* Open a terminal or command prompt and navigate to project directory: `src/WebSearchIndexing`
* Run the following command to set the user secrets for project:
```
dotnet user-secrets set "ConnectionStrings:IndexingDb" "Host=localhost;Port=5432;Username=postgres;Password=your_postgres_password;Database=IndexingDb"
```
Replace `your_postgres_password` with your PostgreSQL database password.
* Then, set the application access key:
```
dotnet user-secrets set "ApplicationAccessKey" "your_application_access_key"
```
Replace `your_application_access_key` with the desired access key for your application.

3. Verify User Secrets:

You can verify that the secrets are set correctly by running: `dotnet user-secrets list`

This command will display the list of user secrets stored for your project.
