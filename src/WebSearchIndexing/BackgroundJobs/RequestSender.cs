﻿using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Indexing.v3;
using Google.Apis.Indexing.v3.Data;
using Google.Apis.Services;
using WebSearchIndexing.Domain.Entities;

namespace WebSearchIndexing.BackgroundJobs;

public static class RequestSender
{
    public static bool SendSingleRequest(ServiceAccount serviceAccount, UrlRequest urlRequest, CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            string credentialJson = serviceAccount.Json;
            GoogleCredential credential;
            try
            {
                credential = GoogleCredential
                    .FromJson(credentialJson)
                    .CreateScoped(IndexingService.Scope.Indexing);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading service account credentials", ex);
            }

            IndexingService service = new(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Web search indexing",
            });

            UrlNotification notification = new()
            {
                Url = urlRequest.Url,
                Type = urlRequest.Type switch
                {
                    UrlRequestType.Updated => "URL_UPDATED",
                    UrlRequestType.Deleted => "URL_DELETED",
                    _ => "URL_UPDATED"
                }
            };

            try
            {
                service.UrlNotifications.Publish(notification).Execute();
            }
            catch (GoogleApiException ex)
            {
                throw new Exception($"Error sending request to Google API for URL {urlRequest.Url}", ex);
            }

            return true;
        }

        return false;
    }
}