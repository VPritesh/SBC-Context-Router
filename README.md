## <p align="center">🌐 SBC Context Router</p>
<p align="center">
<strong>A High-Performance, Vendor-Agnostic SIP Gateway for Voice AI & External Agent Integration</strong>
</p>

<p align="center">
<img src="https://img.shields.io/badge/Asterisk-20_LTS-orange?style=for-the-badge&logo=asterisk" alt="Asterisk">
<img src="https://img.shields.io/badge/AWS-EC2_C6i-232F3E?style=for-the-badge&logo=amazon-aws" alt="AWS">
<img src="https://img.shields.io/badge/Ubuntu-LTS-E95420?style=for-the-badge&logo=ubuntu" alt="Ubuntu">
<img src="https://img.shields.io/badge/Cloudflare-DNS_Only-F38020?style=for-the-badge&logo=cloudflare" alt="Cloudflare">
</p>



# 📖 Overview

This project provides a robust, production-ready configuration for an Asterisk-based Session Border Controller (SBC). It is specifically engineered to solve the "Session Context Loss" problem when bridging internal telephony stacks with External Voice AI Agents or Third-Party IVRs. By implementing a Smart Memory Engine, this SBC captures and retains campaign attribution data before forwarding calls to external vendors, ensuring 100% routing accuracy during human-agent escalations.

# 🚀 Key Features

## 🧠 Stateful Context Memory
- Intelligent Attribution: Captures and maps Caller IDs to specific escalation queues via an internal SQLite data store.
- Accurate Escalation: Ensures calls returning from a Voice AI are routed back to the exact original department, eliminating "blind transfers"

## 🛠️ Zero-Middleware Architecture
- Independence: Removes the requirement for proprietary third-party SIP proxies or expensive licensed SBC software.
- Custom Compiled: Built on Asterisk 20 LTS for maximum stability and control.

## ⚡ High-Density Performance

- Scalability: Designed to handle up to 100,000 daily calls using AWS C5/C6i compute instances.
- Cost Optimization: Uses a "Pass-Through" signaling model with no local audio recording, requiring only a 20GB storage footprint.

## 🛡️ Carrier-Grade Security & Maintenance

- Perimeter Defense: Locked down via AWS Security Groups and Cloudflare "DNS Only" (Grey Cloud) to prevent unauthorized SIP probes.
- Self-Healing: Includes automated weekly system flushes and daily log rotation with 30-day retention policies.

# 🏗️ Technical Architecture
The SBC acts as the "Brain" between the Primary PBX and the External Voice Platform:
1. Ingress: Primary PBX sends a call with a campaign-specific prefix.
2. Memory Capture: SBC strips the prefix and saves the CallerID -> CampaignID mapping.
3. Handoff: SBC initiates a SIP INVITE to the External Voice Agent.
4. Escalation: When the Voice Agent transfers back, the SBC retrieves the mapping and routes the call to the live human queue


# 🛠️ Deployment Summary
    ---------------------------------------------------------
    |       Component       |           Specification       |
    ---------------------------------------------------------
    |   Cloud Provider      |   AWS (Amazon Web Services)   |
    |   Instance Type       |   m6i.large/ c6i.large        |
    |   OS                  |   OS,Ubuntu Linux LTS         |
    |   Storage             |   20GB gp3 EBS Volume         |
    |   SIP Signaling       |   UDP 5060 (Restricted IPs)   |
    |   RTP Media           |   UDP 10000-20000             |
    ---------------------------------------------------------


# 🔍 Observability
Monitor your traffic in real-time with integrated tools:
- Visual SIP Inspection: sngrep for packet analysis.
- Live Logic Tracing: `````tail -f /var/log/asterisk/full ````` to watch routing decisions.
- CLI Monitoring: `````asterisk -rvvvvv````` for detailed channel status.

<p align="center">
<sub>Managed and Architected by <strong>Pritesh Vadhel</strong> • 2026</sub>
</p>
