﻿// <copyright file="EditRequestCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.RemoteSupport.Cards
{
    using System;
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Localization;
    using Microsoft.Teams.Apps.RemoteSupport.Common;
    using Microsoft.Teams.Apps.RemoteSupport.Common.Models;
    using Microsoft.Teams.Apps.RemoteSupport.Helpers;
    using Microsoft.Teams.Apps.RemoteSupport.Models;
    using Newtonsoft.Json;

    /// <summary>
    /// Class holds card for Edit request.
    /// </summary>
    public class EditRequestCard : IEditRequestCard
    {
        /// <summary>
        /// Helper that handles the card configuration.
        /// </summary>
        private readonly ICardHelper cardHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditRequestCard"/> class.
        /// </summary>
        /// <param name="cardHelper">Microsoft Application credentials for Bot/ME.</param>
        public EditRequestCard(ICardHelper cardHelper)
        {
            this.cardHelper = cardHelper;
        }

        /// <summary>
        /// Gets Edit card for task module.
        /// </summary>
        /// <param name="ticketDetail">Ticket details from user.</param>
        /// <param name="cardConfiguration">Card configuration.</param>
        /// <param name="localizer">The current cultures' string localizer.</param>
        /// <param name="existingTicketDetail">Existing ticket details.</param>
        /// <returns>Returns an attachment of edit card.</returns>
        public Attachment GetEditRequestCard(TicketDetail ticketDetail, CardConfigurationEntity cardConfiguration, IStringLocalizer<Strings> localizer, TicketDetail existingTicketDetail = null)
        {
            cardConfiguration = cardConfiguration ?? throw new ArgumentNullException(nameof(cardConfiguration));
            ticketDetail = ticketDetail ?? throw new ArgumentNullException(nameof(ticketDetail));

            string issueTitle = string.Empty;
            string issueDescription = string.Empty;
            var dynamicElements = new List<AdaptiveElement>();
            var ticketAdditionalFields = new List<AdaptiveElement>();
            bool showTitleValidation = false;
            bool showDescriptionValidation = false;
            bool showDateValidation = false;

            if (string.IsNullOrWhiteSpace(ticketDetail.Title))
            {
                showTitleValidation = true;
            }
            else
            {
                issueTitle = ticketDetail.Title;
            }

            if (string.IsNullOrWhiteSpace(ticketDetail.Description))
            {
                showDescriptionValidation = true;
            }
            else
            {
                issueDescription = ticketDetail.Description;
            }

            var ticketAdditionalDetails = JsonConvert.DeserializeObject<Dictionary<string, string>>(ticketDetail.AdditionalProperties);
            ticketAdditionalFields = this.cardHelper.ConvertToAdaptiveCard(localizer, cardConfiguration.CardTemplate, showDateValidation, ticketAdditionalDetails);

            dynamicElements.AddRange(new List<AdaptiveElement>
            {
                new AdaptiveTextBlock()
                {
                    Text = localizer.GetString("TitleDisplayText"),
                    Spacing = AdaptiveSpacing.Medium,
                },
                new AdaptiveTextInput()
                {
                    Id = "Title",
                    MaxLength = 100,
                    Placeholder = localizer.GetString("TitlePlaceHolderText"),
                    Spacing = AdaptiveSpacing.Small,
                    Value = issueTitle,
                },
                new AdaptiveTextBlock()
                {
                    Text = localizer.GetString("TitleValidationText"),
                    Spacing = AdaptiveSpacing.None,
                    IsVisible = showTitleValidation,
                    Color = AdaptiveTextColor.Attention,
                },
                new AdaptiveTextBlock()
                {
                    Text = localizer.GetString("DescriptionText"),
                    Spacing = AdaptiveSpacing.Medium,
                },
                new AdaptiveTextInput()
                {
                    Id = "Description",
                    MaxLength = 500,
                    IsMultiline = true,
                    Placeholder = localizer.GetString("DesciptionPlaceHolderText"),
                    Spacing = AdaptiveSpacing.Small,
                    Value = issueDescription,
                },
                new AdaptiveTextBlock()
                {
                    Text = localizer.GetString("DescriptionValidationText"),
                    Spacing = AdaptiveSpacing.None,
                    IsVisible = showDescriptionValidation,
                    Color = AdaptiveTextColor.Attention,
                },
                new AdaptiveTextBlock()
                {
                    Text = localizer.GetString("RequestTypeText"),
                    Spacing = AdaptiveSpacing.Medium,
                },
                new AdaptiveChoiceSetInput
                {
                    Choices = new List<AdaptiveChoice>
                    {
                        new AdaptiveChoice
                        {
                            Title = localizer.GetString("NormalText"),
                            Value = localizer.GetString("NormalText"),
                        },
                        new AdaptiveChoice
                        {
                            Title = localizer.GetString("UrgentText"),
                            Value = localizer.GetString("UrgentText"),
                        },
                    },
                    Id = "RequestType",
                    Value = !string.IsNullOrEmpty(ticketDetail?.RequestType) ? ticketDetail?.RequestType : localizer.GetString("NormalText"),
                    Style = AdaptiveChoiceInputStyle.Expanded,
                },
            });

            dynamicElements.AddRange(ticketAdditionalFields);

            AdaptiveCard ticketDetailsPersonalChatCard = new AdaptiveCard(Constants.AdaptiveCardVersion)
            {
                Body = dynamicElements,
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = localizer.GetString("UpdateActionText"),
                        Id = "UpdateRequest",
                        Data = new AdaptiveCardAction
                        {
                            Command = Constants.UpdateRequestAction,
                            TeamId = cardConfiguration?.TeamId,
                            TicketId = ticketDetail.TicketId,
                            CardId = ticketDetail.CardId,
                        },
                    },
                    new AdaptiveSubmitAction
                    {
                        Title = localizer.GetString("CancelButtonText"),
                        Id = "Cancel",
                        Data = new AdaptiveCardAction
                        {
                            Command = Constants.CancelCommand,
                        },
                    },
                },
            };
            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = ticketDetailsPersonalChatCard,
            };
        }

        /// <summary>
        /// Construct the card to render error message text to task module.
        /// </summary>
        /// <param name="localizer">The current cultures' string localizer.</param>
        /// <returns>Card attachment.</returns>
        public Attachment GetClosedErrorCard(IStringLocalizer<Strings> localizer)
        {
            AdaptiveCard closedErrorCard = new AdaptiveCard(new AdaptiveSchemaVersion(Constants.AdaptiveCardVersion))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                        Text = localizer.GetString("ClosedErrorMessage"),
                        Wrap = true,
                    },
                },
            };
            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = closedErrorCard,
            };
        }
    }
}